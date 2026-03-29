namespace AiTest.Core.Messages;

using AiTest.Core.Models;

public enum GamePhaseType
{
    Programming,
    Execution,
    GameOver
}

public sealed record CardSelectedEvent(int PlayerIndex, int CardIndex, int Register) : IEvent;

public sealed record RegisterSelectionChangedEvent(int SelectedRegister) : IEvent;

public sealed record PlayerConfirmedEvent(int PlayerIndex) : IEvent;

public sealed record RegisterExecutedEvent(int Register, int PlayerIndex) : IEvent;

public sealed record PhaseChangedEvent(GamePhaseType Phase, int Round) : IEvent;

public sealed record ExecutionStartedEvent(int Round) : IEvent;

public sealed record RegistersClearedEvent() : IEvent;

public sealed record MovementPhaseStartedEvent(int Register) : IEvent;

public sealed record ConveyorPhaseStartedEvent(int Register) : IEvent;

public sealed record LaserPhaseStartedEvent(int Register) : IEvent;

public sealed record PhaseExecutionCompleteEvent(int Register) : IEvent;

public sealed record MoveEvent(int PlayerId, Position From, Position To) : IEvent;

public sealed record RotateEvent(int PlayerId, Direction From, Direction To) : IEvent;

public sealed record LaserFireEvent(int SourceId, Position Source, Direction Direction, IReadOnlyList<Position> Path, int? HitPlayerId, int Damage) : IEvent;

public sealed record DamageEvent(int PlayerId, int Damage, int NewHealth) : IEvent;

public sealed record ConveyorMoveEvent(int PlayerId, Position From, Position To) : IEvent;

public sealed record CheckpointEvent(int PlayerId, int CheckpointNumber) : IEvent;

public sealed record RobotDestroyedEvent(int PlayerId, Position LastPosition) : IEvent;

public sealed record RobotRebootEvent(int PlayerId, Position SpawnPosition) : IEvent;

public sealed record RobotRepairedEvent(int PlayerId, int RepairAmount, int NewDamage) : IEvent;

public sealed record LifeTokenLostEvent(int PlayerId, int RemainingTokens) : IEvent;

public sealed record PowerDownToggledEvent(int PlayerIndex, bool IsPowerDown) : IEvent;
