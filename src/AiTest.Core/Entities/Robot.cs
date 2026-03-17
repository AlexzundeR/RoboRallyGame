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
    public string Name { get; set; } = "Robot";
    public Position Position { get; set; }
    public Direction Direction { get; set; } = Direction.North;
    public int Damage { get; set; }
    public int Energy { get; set; } = 3;
    public int LifeTokens { get; set; } = 3;
    public RobotStatus Status { get; set; } = RobotStatus.Active;
    public int CheckpointsReached { get; set; }
    public bool IsAlive => Status != RobotStatus.Destroyed;

    public int LockedRegisters => Damage >= 5 ? Math.Min(Damage - 4, 5) : 0;
    public bool IsPowerDown => Status == RobotStatus.PowerDown;
    public bool NeedsReboot => Status == RobotStatus.Rebooting;

    public void TakeDamage(int amount)
    {
        Damage += amount;
        if (Damage >= 10)
        {
            if (LifeTokens > 0)
            {
                LifeTokens--;
                Status = RobotStatus.Rebooting;
                Damage = 0;
            }
            else
            {
                Status = RobotStatus.Destroyed;
            }
        }
    }

    public void Move(Position newPos)
    {
        Position = newPos;
    }

    public void RotateLeft() => Direction = Direction.TurnLeft();
    public void RotateRight() => Direction = Direction.TurnRight();
    public void Rotate180() => Direction = Direction.TurnAround();

    public void Reboot(Position spawnPos)
    {
        Position = spawnPos;
        Direction = Direction.North;
        Status = RobotStatus.Active;
        Damage = 2;
        CheckpointsReached = 0;
    }

    public void PowerDown()
    {
        Status = RobotStatus.PowerDown;
    }

    public void WakeUp()
    {
        Status = RobotStatus.Active;
    }
}
