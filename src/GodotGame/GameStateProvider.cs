namespace AiTest.Godot;

using System.Collections.Generic;
using System.Linq;
using AiTest.Core.Entities;
using AiTest.Core.Engine;
using AiTest.Core.Models;
using AiTest.Core.Cards;

public sealed record CardDto(string Symbol, int Priority, int CardNumber);

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
    int LockedRegisters,
    IReadOnlyList<CardDto?> Hand,
    IReadOnlyList<RegisterCardDto?> Registers,
    int FilledRegisterCount,
    int AvailableCardCount
);

public sealed record RegisterCardDto(CardDto Card, int HandIndex);

public sealed record StateDto(
    IReadOnlyList<PlayerDto> Players,
    int CurrentPlayerIndex,
    int CurrentRegister,
    int Round,
    GamePhase Phase,
    int MapWidth,
    int MapHeight
);

public interface IGameProvider
{
    StateDto GetState();
    PlayerDto? GetPlayer(int playerId);
    PlayerDto? GetCurrentPlayer();
    void SetCurrentRegister(int register);
    void SetCurrentPlayerIndex(int index);
    void SetPhase(GamePhase phase);
    void GrantDamage(int playerId, int amount);
    void GrantRandomUpgrade(int playerId);
    void StartGame();
    Core.Engine.StepResult TryExecuteNextStep();
    void NextRegister();
    void NextRound();
    bool IsGameOver();
    IReadOnlyList<Player> GetWinners();
    CardDto? GetHandCard(int cardIndex);
    void AssignCardToRegister(int handIndex, int registerIndex);
    bool CanConfirmRegistration();
    void TogglePowerDown();
}

public sealed class LocalGameProvider : IGameProvider
{
    private static CardDto? ToCardDto(ProgramCard? card) => card == null ? null : new CardDto(card.Symbol, card.Priority, 0);
    private static RegisterCardDto? ToRegisterCardDto(RegisterCard? rc) => rc == null ? null : new RegisterCardDto(ToCardDto(rc.Card)!, rc.HandIndex);
    private static PlayerDto ToPlayerDto(Player p) => new PlayerDto(
        p.Id, p.Name, p.Robot.Position, p.Robot.Direction, p.Robot.Damage, p.Robot.LifeTokens,
        p.Robot.IsPowerDown, p.Robot.IsAlive, p.Robot.CheckpointsReached, p.Robot.LockedRegisters,
        p.Hand.Select(ToCardDto).ToList(), p.Registers.Select(ToRegisterCardDto).ToList(),
        p.FilledRegisterCount, p.AvailableCardCount);

    private readonly IGameEngine _engine;

    public LocalGameProvider(IGameEngine engine) => _engine = engine;

    public StateDto GetState()
    {
        var s = _engine.State;
        return new StateDto(s.Players.Select(ToPlayerDto).ToList(), s.CurrentPlayerIndex, s.CurrentRegister, s.Round, s.Phase, s.Map.Width, s.Map.Height);
    }

    public PlayerDto? GetPlayer(int playerId) => ToPlayerDto(_engine.State.Players.FirstOrDefault(p => p.Id == playerId)!);
    public PlayerDto? GetCurrentPlayer() => ToPlayerDto(_engine.State.GetCurrentPlayer()!);
    public void SetCurrentRegister(int r) => _engine.SetCurrentRegister(r);
    public void SetCurrentPlayerIndex(int i) => _engine.SetCurrentPlayerIndex(i);
    public void SetPhase(GamePhase p) => _engine.SetPhase(p);
    public void GrantDamage(int playerId, int amount) => _engine.GrantDamage(playerId, amount);
    public void GrantRandomUpgrade(int playerId) => _engine.GrantRandomUpgrade(playerId);
    public void StartGame() => _engine.StartGame();
    public Core.Engine.StepResult TryExecuteNextStep() => _engine.TryExecuteNextStep();
    public void NextRegister() => _engine.NextRegister();
    public void NextRound() => _engine.NextRound();
    public bool IsGameOver() => _engine.IsGameOver();
    public IReadOnlyList<Player> GetWinners() => _engine.GetWinners();
    public CardDto? GetHandCard(int cardIndex) => ToCardDto(_engine.State.GetCurrentPlayer()?.GetHandCard(cardIndex));
    public void AssignCardToRegister(int handIndex, int registerIndex) => _engine.State.GetCurrentPlayer()?.AssignCardToRegister(handIndex, registerIndex);
    public bool CanConfirmRegistration() => _engine.State.GetCurrentPlayer()?.CanConfirmRegistration() ?? false;
    public void TogglePowerDown() => _engine.State.GetCurrentPlayer()?.TogglePowerDown();
}