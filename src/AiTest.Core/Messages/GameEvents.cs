namespace AiTest.Core.Messages;

public enum GamePhaseType
{
    Programming,
    Execution,
    GameOver
}

public sealed record CardSelectedEvent(int PlayerIndex, int CardIndex, int Register) : IEvent;

public sealed record PlayerConfirmedEvent(int PlayerIndex) : IEvent;

public sealed record RegisterExecutedEvent(int Register, int PlayerIndex) : IEvent;

public sealed record PhaseChangedEvent(GamePhaseType Phase, int Round) : IEvent;
