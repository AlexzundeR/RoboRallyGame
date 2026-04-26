namespace AiTest.Godot.Messages;

using System.Collections.Generic;

public enum GamePhaseType
{
    Programming,
    Execution,
    GameOver
}

public sealed record CardSelectedEvent(int PlayerIndex, int CardIndex, int Register);

public sealed record RegisterSelectionChangedEvent(int SelectedRegister);

public sealed record PlayerConfirmedEvent(int PlayerIndex);

public sealed record RegisterExecutedEvent(int Register, int PlayerIndex);

public sealed record PhaseChangedEvent(GamePhaseType Phase, int Round);

public sealed record ExecutionStartedEvent(int Round);

public sealed record RegistersClearedEvent();

public sealed record MovementPhaseStartedEvent(int Register);

public sealed record ConveyorPhaseStartedEvent(int Register);

public sealed record LaserPhaseStartedEvent(int Register);

public sealed record PhaseExecutionCompleteEvent(int Register);

public sealed record MoveEvent(int PlayerId, int FromX, int FromY, int ToX, int ToY);

public sealed record RotateEvent(int PlayerId, int FromIndex, int ToIndex);

public sealed record LaserFireEvent(int SourceId, int SourceX, int SourceY, int DirectionIndex, int? HitPlayerId, int Damage);

public sealed record DamageEvent(int PlayerId, int Damage, int NewHealth);

public sealed record ConveyorMoveEvent(int PlayerId, int FromX, int FromY, int ToX, int ToY);

public sealed record CheckpointEvent(int PlayerId, int CheckpointNumber, int TotalCheckpoints);

public sealed record RobotDestroyedEvent(int PlayerId, int LastX, int LastY);

public sealed record RobotRebootEvent(int PlayerId, int SpawnX, int SpawnY);

public sealed record RobotRepairedEvent(int PlayerId, int RepairAmount, int NewDamage);

public sealed record LifeTokenLostEvent(int PlayerId, int LifeTokens);

public sealed record GameOverEvent(IReadOnlyList<int> WinnerIds, bool IsTie);

public sealed record PowerDownToggledEvent(int PlayerIndex, bool IsPowerDown);