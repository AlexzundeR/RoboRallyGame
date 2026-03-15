namespace AiTest.Core.Engine;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public enum GamePhase
{
    Setup,
    Programming,
    Activation,
    Cleanup,
    GameOver
}

public sealed class GameState
{
    public GameMap Map { get; set; } = null!;
    public List<Player> Players { get; } = new();
    public int CurrentPlayerIndex { get; set; }
    public int CurrentRegister { get; set; }
    public GamePhase Phase { get; set; } = GamePhase.Setup;
    public int Round { get; set; }
    public Player? Winner { get; set; }

    public Player AddPlayer(string name)
    {
        var player = new Player
        {
            Id = Players.Count + 1,
            Name = name,
            Robot = new Robot
            {
                Id = Players.Count + 1,
                Name = name
            }
        };
        Players.Add(player);
        return player;
    }

    public Player? GetCurrentPlayer()
    {
        return CurrentPlayerIndex >= 0 && CurrentPlayerIndex < Players.Count
            ? Players[CurrentPlayerIndex]
            : null;
    }

    public void Clear()
    {
        Players.Clear();
        CurrentPlayerIndex = 0;
        CurrentRegister = 0;
        Round = 0;
        Phase = GamePhase.Setup;
        Winner = null;
    }
}

public interface IGameEngine
{
    GameState State { get; }
    void Initialize(GameMap map);
    void AddPlayer(string name);
    void StartGame();
    void ExecuteRegister(int registerIndex);
    void ProcessBoardElements();
    void NextRegister();
    void NextRound();
    bool IsGameOver();
    Player? GetWinner();
}

public sealed class GameEngine : IGameEngine
{
    private readonly Random _random = new();

    public GameState State { get; } = new();

    public void Initialize(GameMap map)
    {
        State.Map = map;
        State.Phase = GamePhase.Programming;
    }

    public void AddPlayer(string name)
    {
        var player = State.AddPlayer(name);
        var startTiles = FindStartTiles();
        if (startTiles.Count > player.Id - 1)
        {
            player.Robot.Position = startTiles[player.Id - 1];
        }
    }

    public void StartGame()
    {
        State.Phase = GamePhase.Programming;
        State.Round = 1;
        DealCardsToAllPlayers();
    }

    public void ExecuteRegister(int registerIndex)
    {
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive || player.Robot.NeedsReboot)
                continue;

            if (registerIndex < player.Robot.LockedRegisters)
                continue;

            var card = player.GetRegister(registerIndex);
            card?.Execute(player.Robot, State.Map);
        }

        ProcessBoardElements();
    }

    public void ProcessBoardElements()
    {
        foreach (var player in State.Players)
        {
            if (!player.Robot.IsAlive)
                continue;

            var tile = State.Map.GetTile(player.Robot.Position);

            switch (tile.Type)
            {
                case TileType.Pit:
                    HandlePit(player);
                    break;
                case TileType.GearCW:
                    player.Robot.RotateRight();
                    break;
                case TileType.GearCCW:
                    player.Robot.RotateLeft();
                    break;
                case TileType.ConveyorCW:
                case TileType.ExpressConveyorCW:
                    MoveOnConveyor(player, tile.ConveyorDir!.Value, tile.Type == TileType.ExpressConveyorCW ? 2 : 1);
                    break;
                case TileType.ConveyorCCW:
                case TileType.ExpressConveyorCCW:
                    MoveOnConveyor(player, tile.ConveyorDir!.Value, tile.Type == TileType.ExpressConveyorCCW ? 2 : 1);
                    break;
                case TileType.Checkpoint:
                    HandleCheckpoint(player, tile.CheckpointNumber ?? 0);
                    break;
            }

            if (tile.HasBoardLaser)
            {
                FireBoardLaser(player);
            }
        }
    }

    private void HandlePit(Player player)
    {
        player.Robot.Status = RobotStatus.Rebooting;
    }

    private void HandleCheckpoint(Player player, int checkpointNum)
    {
        if (checkpointNum == player.Robot.CheckpointsReached + 1)
        {
            player.Robot.CheckpointsReached = checkpointNum;
        }
    }

    private void MoveOnConveyor(Player player, ConveyorDirection dir, int spaces)
    {
        var direction = dir switch
        {
            ConveyorDirection.North => Direction.North,
            ConveyorDirection.East => Direction.East,
            ConveyorDirection.South => Direction.South,
            ConveyorDirection.West => Direction.West,
            _ => Direction.North
        };

        for (int i = 0; i < spaces; i++)
        {
            var newPos = player.Robot.Position.Move(direction);
            if (State.Map.IsInBounds(newPos))
            {
                var targetTile = State.Map.GetTile(newPos);
                if (targetTile.Type != TileType.Wall && targetTile.Type != TileType.Pit)
                {
                    player.Robot.Position = newPos;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private void FireBoardLaser(Player player)
    {
        foreach (var other in State.Players)
        {
            if (other.Id == player.Id || !other.Robot.IsAlive)
                continue;

            if (IsInLaserPath(player.Robot.Position, other.Robot.Position))
            {
                other.Robot.TakeDamage(1);
            }
        }
    }

    private bool IsInLaserPath(Position laserPos, Position targetPos)
    {
        return laserPos.X == targetPos.X || laserPos.Y == targetPos.Y;
    }

    public void NextRegister()
    {
        State.CurrentRegister++;
        if (State.CurrentRegister >= 5)
        {
            State.CurrentRegister = 0;
            NextRound();
        }
    }

    public void NextRound()
    {
        State.Round++;
        State.CurrentRegister = 0;
        State.Phase = GamePhase.Programming;
        
        foreach (var player in State.Players)
        {
            if (player.Robot.Status == RobotStatus.PowerDown)
            {
                player.Robot.WakeUp();
                player.Robot.Damage = 0;
            }
            player.ClearRegisters();
        }

        DealCardsToAllPlayers();
    }

    public bool IsGameOver()
    {
        return State.Players.Any(p => p.Robot.CheckpointsReached >= 4);
    }

    public Player? GetWinner()
    {
        return State.Players.FirstOrDefault(p => p.Robot.CheckpointsReached >= 4);
    }

    private List<Position> FindStartTiles()
    {
        var starts = new List<Position>();
        for (int x = 0; x < State.Map.Width; x++)
        {
            for (int y = 0; y < State.Map.Height; y++)
            {
                if (State.Map.GetTile(x, y).Type == TileType.Start)
                    starts.Add(new Position(x, y));
            }
        }
        return starts;
    }

    private void DealCardsToAllPlayers()
    {
        var deck = CreateDeck();
        
        foreach (var player in State.Players)
        {
            var hand = deck.OrderBy(_ => _random.Next()).Take(9).ToList();
            player.DealCards(hand);
        }
    }

    private List<ProgramCard> CreateDeck()
    {
        var deck = new List<ProgramCard>();

        for (int i = 0; i < 6; i++)
            deck.Add(new MoveCard { Priority = 100 + i, Spaces = 1 });
        
        for (int i = 0; i < 6; i++)
            deck.Add(new MoveCard { Priority = 200 + i, Spaces = 2 });
        
        for (int i = 0; i < 4; i++)
            deck.Add(new MoveCard { Priority = 300 + i, Spaces = 3 });

        for (int i = 0; i < 6; i++)
            deck.Add(new TurnCard { Priority = 150 + i, IsLeft = true });
        
        for (int i = 0; i < 6; i++)
            deck.Add(new TurnCard { Priority = 250 + i, IsLeft = false });

        for (int i = 0; i < 2; i++)
            deck.Add(new UTurnCard { Priority = 350 + i });

        return deck;
    }
}
