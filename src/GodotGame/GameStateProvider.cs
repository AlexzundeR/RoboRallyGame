namespace AiTest.Godot;

using System.Collections.Generic;
using System.Linq;
using AiTest.Core.Entities;
using AiTest.Core.Engine;
using AiTest.Core.Models;

public sealed record PlayerDto(
    int Id,
    string Name,
    Position Position,
    Direction Direction,
    int Damage,
    int LifeTokens,
    bool IsPowerDown,
    bool IsAlive,
    int CheckpointsReached,
    int LockedRegisters
);

public sealed record StateDto(
    IReadOnlyList<PlayerDto> Players,
    int CurrentPlayerIndex,
    int CurrentRegister,
    int Round,
    GamePhase Phase,
    int MapWidth,
    int MapHeight
);

public sealed class GameStateProvider
{
    private readonly IGameEngine _engine;

    public GameStateProvider(IGameEngine engine)
    {
        _engine = engine;
    }

    public StateDto GetState()
    {
        var state = _engine.State;
        return new StateDto(
            state.Players.Select(p => new PlayerDto(
                p.Id,
                p.Name,
                p.Robot.Position,
                p.Robot.Direction,
                p.Robot.Damage,
                p.Robot.LifeTokens,
                p.Robot.IsPowerDown,
                p.Robot.IsAlive,
                p.Robot.CheckpointsReached,
                p.Robot.LockedRegisters
            )).ToList(),
            state.CurrentPlayerIndex,
            state.CurrentRegister,
            state.Round,
            state.Phase,
            state.Map.Width,
            state.Map.Height
        );
    }

    public PlayerDto? GetPlayer(int playerId)
    {
        var player = _engine.State.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return null;
        
        return new PlayerDto(
            player.Id,
            player.Name,
            player.Robot.Position,
            player.Robot.Direction,
            player.Robot.Damage,
            player.Robot.LifeTokens,
            player.Robot.IsPowerDown,
            player.Robot.IsAlive,
            player.Robot.CheckpointsReached,
            player.Robot.LockedRegisters
        );
    }

    public PlayerDto? GetCurrentPlayer()
    {
        var player = _engine.State.GetCurrentPlayer();
        if (player == null) return null;
        
        return new PlayerDto(
            player.Id,
            player.Name,
            player.Robot.Position,
            player.Robot.Direction,
            player.Robot.Damage,
            player.Robot.LifeTokens,
            player.Robot.IsPowerDown,
            player.Robot.IsAlive,
            player.Robot.CheckpointsReached,
            player.Robot.LockedRegisters
        );
    }

    public void SetCurrentRegister(int register) => _engine.SetCurrentRegister(register);
    public void SetCurrentPlayerIndex(int index) => _engine.SetCurrentPlayerIndex(index);
    public void SetPhase(GamePhase phase) => _engine.SetPhase(phase);
    public void GrantDamage(int playerId, int amount) => _engine.GrantDamage(playerId, amount);
    public void GrantRandomUpgrade(int playerId) => _engine.GrantRandomUpgrade(playerId);
    public void StartGame() => _engine.StartGame();
    public StepResult TryExecuteNextStep() => _engine.TryExecuteNextStep();
    public void NextRegister() => _engine.NextRegister();
    public void NextRound() => _engine.NextRound();
    public bool IsGameOver() => _engine.IsGameOver();
    public IReadOnlyList<Player> GetWinners() => _engine.GetWinners();
}
