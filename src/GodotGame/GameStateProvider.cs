namespace RoboRallyGame.Godot;

using System.Collections.Generic;
using System.Linq;

using RoboRallyGame.Core.Entities;
using RoboRallyGame.Godot.Messages;
using RoboRallyGame.Core.Engine;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Core.Cards;
using RoboRallyGame.Godot.Models;

public sealed record CardViewModel(string Symbol, int Priority, int CardNumber);

public sealed record PlayerViewModel(
    int Id,
    string Name,
    int X,
    int Y,
    RoboRallyGame.Godot.Models.Direction Direction,
    int Damage,
    int LifeTokens,
    bool IsPowerDown,
    bool IsAlive,
    int CheckpointsReached,
    int LockedRegisters,
    IReadOnlyList<CardViewModel?> Hand,
    IReadOnlyList<RegisterCardViewModel?> Registers,
    int FilledRegisterCount,
    int AvailableCardCount,
    IReadOnlyList<string> AllUpgrades
);

public sealed record RegisterCardViewModel(CardViewModel Card, int HandIndex);

public sealed record StateViewModel(
    IReadOnlyList<PlayerViewModel> Players,
    int CurrentPlayerIndex,
    int CurrentRegister,
    int Round,
    GamePhaseType Phase,
    int MapWidth,
    int MapHeight
);

public interface IGameProvider
{
    StateViewModel GetState();
    MapData GetMapData();
    PlayerViewModel? GetPlayer(int playerId);
    PlayerViewModel? GetCurrentPlayer();
    IReadOnlyList<PlayerViewModel> GetPlayers();
    void SetCurrentRegister(int register);
    void SetCurrentPlayerIndex(int index);
    void SetPhase(int phase);
    void GrantDamage(int playerId, int amount);
    void GrantRandomUpgrade(int playerId);
    void StartGame();
    StepResult TryExecuteNextStep();
    void NextRegister();
    void NextRound();
    bool IsGameOver();
    int GetWinners();
    CardViewModel? GetHandCard(int cardIndex);
    void AssignCardToRegister(int handIndex, int registerIndex);
    bool CanConfirmRegistration();
    void TogglePowerDown();
}

public sealed class LocalGameProvider : IGameProvider
{
    private static CardViewModel? ToCardViewModel(ProgramCard? card) => card == null ? null : new CardViewModel(card.Symbol, card.Priority, 0);
    private static RegisterCardViewModel? ToRegisterCardViewModel(RegisterCard? rc) => rc == null ? null : new RegisterCardViewModel(ToCardViewModel(rc.Card)!, rc.HandIndex);
    
    private static Models.Direction ToGodotDirection(Core.Models.Direction dir) => dir switch
    {
        Core.Models.Direction.North => Models.Direction.North,
        Core.Models.Direction.East => Models.Direction.East,
        Core.Models.Direction.South => Models.Direction.South,
        Core.Models.Direction.West => Models.Direction.West,
        _ => Models.Direction.North
    };

    private static Models.TileType ToGodotTileType(Core.Models.TileType type) => type switch
    {
        Core.Models.TileType.Floor => Models.TileType.Floor,
        Core.Models.TileType.Wall => Models.TileType.Wall,
        Core.Models.TileType.Pit => Models.TileType.Pit,
        Core.Models.TileType.Conveyor => Models.TileType.Conveyor,
        Core.Models.TileType.Gear => Models.TileType.Gear,
        Core.Models.TileType.Laser => Models.TileType.Laser,
        Core.Models.TileType.Start => Models.TileType.Start,
        Core.Models.TileType.Checkpoint => Models.TileType.Checkpoint,
        _ => Models.TileType.Floor
    };

    private static Models.ConveyorDirection? ToGodotConveyorDirection(Core.Models.ConveyorDirection? dir) => dir switch
    {
        Core.Models.ConveyorDirection.North => Models.ConveyorDirection.North,
        Core.Models.ConveyorDirection.East => Models.ConveyorDirection.East,
        Core.Models.ConveyorDirection.South => Models.ConveyorDirection.South,
        Core.Models.ConveyorDirection.West => Models.ConveyorDirection.West,
        null => null,
        _ => Models.ConveyorDirection.North
    };
    
    private static Messages.GamePhaseType ToGodotPhase(Core.Engine.GamePhase phase) => phase switch
    {
        Core.Engine.GamePhase.Setup => Messages.GamePhaseType.Programming,
        Core.Engine.GamePhase.Programming => Messages.GamePhaseType.Programming,
        Core.Engine.GamePhase.Activation => Messages.GamePhaseType.Execution,
        Core.Engine.GamePhase.Cleanup => Messages.GamePhaseType.Execution,
        Core.Engine.GamePhase.GameOver => Messages.GamePhaseType.GameOver,
        _ => Messages.GamePhaseType.Programming
    };
    
    private static PlayerViewModel ToPlayerViewModel(Player p) => new PlayerViewModel(
        p.Id, p.Name, p.Robot.Position.X, p.Robot.Position.Y, ToGodotDirection(p.Robot.Direction), 
        p.Robot.Damage, p.Robot.LifeTokens,
        p.Robot.IsPowerDown, p.Robot.IsAlive, p.Robot.CheckpointsReached, p.Robot.LockedRegisters,
        p.Hand.Select(ToCardViewModel).ToList(), p.Registers.Select(ToRegisterCardViewModel).ToList(),
        p.FilledRegisterCount, p.AvailableCardCount,
        p.AllUpgrades.Select(u => u.ToString()).ToList());

    private readonly IGameEngine _engine;

    public LocalGameProvider(IGameEngine engine) => _engine = engine;

    private static List<int> BlockedToList(Core.Models.DirectionSet ds)
    {
        var list = new List<int>();
        if (ds.Contains(Core.Models.Direction.North)) list.Add(0);
        if (ds.Contains(Core.Models.Direction.East)) list.Add(1);
        if (ds.Contains(Core.Models.Direction.South)) list.Add(2);
        if (ds.Contains(Core.Models.Direction.West)) list.Add(3);
        return list;
    }

    private static TileData ToTileData(Core.Models.Tile tile)
    {
        var conveyor = tile.GetComponent<Core.Models.Components.ConveyorBehaviorComponent>();
        var gear = tile.GetComponent<Core.Models.Components.GearBehaviorComponent>();
        var blocked = tile.GetComponent<Core.Models.Components.BlockedMovementComponent>();

        return new TileData(
            tile.Position.X,
            tile.Position.Y,
            ToGodotTileType(tile.Type),
            tile.CheckpointNumber,
            ToGodotConveyorDirection(conveyor?.Direction),
            conveyor?.IsExpress,
            gear?.IsClockwise,
            blocked != null ? BlockedToList(blocked.BlockedDirections) : null
        );
    }

    public MapData GetMapData()
    {
        var map = _engine.State.Map;
        var tiles = new TileData[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                tiles[x, y] = ToTileData(map.GetTile(new Position(x, y)));
            }
        }

        return new MapData(map.Width, map.Height, tiles);
    }

    public StateViewModel GetState()
    {
        var s = _engine.State;
        return new StateViewModel(s.Players.Select(ToPlayerViewModel).ToList(), s.CurrentPlayerIndex, s.CurrentRegister, s.Round, ToGodotPhase(s.Phase), s.Map.Width, s.Map.Height);
    }

    public PlayerViewModel? GetPlayer(int playerId) => ToPlayerViewModel(_engine.State.Players.FirstOrDefault(p => p.Id == playerId)!);
    public PlayerViewModel? GetCurrentPlayer() => ToPlayerViewModel(_engine.State.GetCurrentPlayer()!);
    public IReadOnlyList<PlayerViewModel> GetPlayers() => _engine.State.Players.Select(ToPlayerViewModel).ToList();
    public void SetCurrentRegister(int r) => _engine.SetCurrentRegister(r);
    public void SetCurrentPlayerIndex(int i) => _engine.SetCurrentPlayerIndex(i);
    public void SetPhase(int p) => _engine.SetPhase((GamePhase)p);
    public void GrantDamage(int playerId, int amount) => _engine.GrantDamage(playerId, amount);
    public void GrantRandomUpgrade(int playerId) => _engine.GrantRandomUpgrade(playerId);
    public void StartGame() => _engine.StartGame();
    public StepResult TryExecuteNextStep() => _engine.TryExecuteNextStep();
    public void NextRegister() => _engine.NextRegister();
    public void NextRound() => _engine.NextRound();
    public bool IsGameOver() => _engine.IsGameOver();
    public int GetWinners() => _engine.GetWinners()[0].Id;
    public CardViewModel? GetHandCard(int cardIndex) => ToCardViewModel(_engine.State.GetCurrentPlayer()?.GetHandCard(cardIndex));
    public void AssignCardToRegister(int handIndex, int registerIndex) => _engine.State.GetCurrentPlayer()?.AssignCardToRegister(handIndex, registerIndex);
    public bool CanConfirmRegistration() => _engine.State.GetCurrentPlayer()?.CanConfirmRegistration() ?? false;
    public void TogglePowerDown() => _engine.State.GetCurrentPlayer()?.TogglePowerDown();
}