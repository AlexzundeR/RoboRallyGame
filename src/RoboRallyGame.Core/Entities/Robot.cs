namespace RoboRallyGame.Core.Entities;

using RoboRallyGame.Core.Models;

public sealed class Robot
{
    private IRobotLifecycleComponent _lifecycle = DefaultRobotLifecycle.Instance;
    
    public int Id { get; init; }
    public string Name { get; init; } = "Robot";
    public Position Position { get; private set; }
    public Position StartPosition { get; private set; }
    public Direction Direction { get; private set; } = Direction.North;
    public int Damage { get; private set; }
    public int Energy { get; private set; } = 3;
    public int LifeTokens { get; private set; } = 3;
    public RobotStatus Status { get; private set; } = RobotStatus.Active;
    public int CheckpointsReached { get; private set; }
    public Position? LastCheckpointPosition { get; private set; }
    
    private readonly HashSet<int> _collectedCheckpoints = new();
    
    public void SetLifecycle(IRobotLifecycleComponent component) => _lifecycle = component;

    public IReadOnlyCollection<int> CollectedCheckpoints => _collectedCheckpoints;

    public bool IsAlive => Status != RobotStatus.Destroyed;
    public int LockedRegisters => Damage >= 5 ? Math.Min(Damage - 4, 5) : 0;
    public void SetLifeTokens(int tokens) => LifeTokens = tokens;
    public void SetDamage(int damage) => Damage = damage;
    public void ClearCheckpoints() => _collectedCheckpoints.Clear();
    public void SetCheckpoints(int count) => CheckpointsReached = count;
    
    public bool IsPowerDown => Status == RobotStatus.PowerDown;
    public bool NeedsReboot => Status == RobotStatus.Rebooting;

    public Robot() { }

    public Robot(Direction direction)
    {
        Direction = direction;
    }

    public Robot(Position position, Direction direction)
    {
        Position = position;
        StartPosition = position;
        Direction = direction;
    }

    public Robot(int id, Position position, Direction direction)
    {
        Id = id;
        Position = position;
        StartPosition = position;
        Direction = direction;
    }

    public void SetPosition(Position newPos) => Position = newPos;
    public void SetStartPosition(Position pos) => StartPosition = pos;
    public void Move(Position newPos) => Position = newPos;
    public void SetDirection(Direction direction) => Direction = direction;
    public void ClearLastCheckpoint() => LastCheckpointPosition = null;
    public void RotateLeft() => Direction = Direction.TurnLeft();
    public void RotateRight() => Direction = Direction.TurnRight();
    public void Rotate180() => Direction = Direction.TurnAround();

    public void TakeDamage(int amount) => _lifecycle.OnTakeDamage(this, amount);
    public void CheckDamage() => _lifecycle.OnAfterDamage(this, Damage);
    public RoundPreparationResult PrepareForRound(Position defaultSpawn) => _lifecycle.PrepareForRound(this, defaultSpawn);
    public void Repair(int amount) => _lifecycle.OnRepair(this, amount);
    public void LoseLifeToken() => _lifecycle.OnLoseLifeToken(this);
    public void Reboot(Position spawnPos) => _lifecycle.OnReboot(this, spawnPos);
    public void SetStatus(RobotStatus newStatus) => Status = newStatus;
    public void PowerDown() => _lifecycle.OnPowerDown(this);
    public void WakeUp() => _lifecycle.OnWakeUp(this);
    
    public void AddEnergy(int amount) => Energy = Math.Min(Energy + amount, 10);
    public void SpendEnergy(int amount) => Energy = Math.Max(Energy - amount, 0);

    public bool HasCheckpoint(int number) => _collectedCheckpoints.Contains(number);

    public void AddCheckpoint(int number, Position pos)
    {
        if (_collectedCheckpoints.Contains(number))
            return;
            
        _collectedCheckpoints.Add(number);
        CheckpointsReached++;
        LastCheckpointPosition = pos;
    }
    
    public void SetCheckpointsReached(int count) => CheckpointsReached = count;
}
