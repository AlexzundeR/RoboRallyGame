namespace AiTest.Core.Entities;

using AiTest.Core.Models;

public enum RobotStatus
{
    Active,
    PowerDown,
    Rebooting,
    Destroyed
}

public sealed class Robot
{
    public int Id { get; init; }
    public string Name { get; init; } = "Robot";
    public Position Position { get; private set; }
    public Direction Direction { get; private set; } = Direction.North;
    public int Damage { get; private set; }
    public int Energy { get; private set; } = 3;
    public int LifeTokens { get; private set; } = 3;
    public RobotStatus Status { get; private set; } = RobotStatus.Active;
    public int CheckpointsReached { get; private set; }

    public bool IsAlive => Status != RobotStatus.Destroyed;
    public int LockedRegisters => Damage >= 5 ? Math.Min(Damage - 4, 5) : 0;
    public bool IsPowerDown => Status == RobotStatus.PowerDown;
    public bool NeedsReboot => Status == RobotStatus.Rebooting;

    public Robot() { }

    public Robot(Position position, Direction direction)
    {
        Position = position;
        Direction = direction;
    }

    public Robot(int id, Position position, Direction direction)
    {
        Id = id;
        Position = position;
        Direction = direction;
    }

    public void SetPosition(Position newPos) => Position = newPos;
    public void Move(Position newPos) => Position = newPos;

    public void SetDirection(Direction direction) => Direction = direction;
    public void RotateLeft() => Direction = Direction.TurnLeft();
    public void RotateRight() => Direction = Direction.TurnRight();
    public void Rotate180() => Direction = Direction.TurnAround();

    public void AddDamage(int amount) => TakeDamage(amount);

    public void TakeDamage(int amount)
    {
        Damage += amount;
        if (Damage >= 10)
        {
            if (LifeTokens > 0)
                LoseLifeToken();
            else
                Status = RobotStatus.Destroyed;
        }
    }

    public void AddEnergy(int amount) => Energy = Math.Min(Energy + amount, 10);
    public void SpendEnergy(int amount) => Energy = Math.Max(Energy - amount, 0);

    public void LoseLifeToken()
    {
        LifeTokens--;
        Status = RobotStatus.Rebooting;
        Damage = 0;
    }

    public void Reboot(Position spawnPos)
    {
        Position = spawnPos;
        Direction = Direction.North;
        Status = RobotStatus.Active;
        Damage = 2;
        CheckpointsReached = 0;
    }

    public void SetStatus(RobotStatus newStatus) => Status = newStatus;
    public void PowerDown() => Status = RobotStatus.PowerDown;
    public void WakeUp() => Status = RobotStatus.Active;

    public void AddCheckpoint() => CheckpointsReached++;
    public void SetCheckpointsReached(int count) => CheckpointsReached = count;
}
