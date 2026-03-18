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
    private Position _position;
    private Direction _direction = Direction.North;
    private int _damage;
    private int _energy = 3;
    private int _lifeTokens = 3;
    private RobotStatus _status = RobotStatus.Active;
    private int _checkpointsReached;

    public int Id { get; init; }
    public string Name { get; init; } = "Robot";

    public Position Position
    {
        get => _position;
        init => _position = value;
    }

    public Direction Direction
    {
        get => _direction;
        init => _direction = value;
    }

    public int Damage
    {
        get => _damage;
        private set => _damage = value;
    }

    public int Energy
    {
        get => _energy;
        private set => _energy = value;
    }

    public int LifeTokens
    {
        get => _lifeTokens;
        private set => _lifeTokens = value;
    }

    public RobotStatus Status
    {
        get => _status;
        private set => _status = value;
    }

    public int CheckpointsReached
    {
        get => _checkpointsReached;
        private set => _checkpointsReached = value;
    }

    public bool IsAlive => Status != RobotStatus.Destroyed;
    public int LockedRegisters => Damage >= 5 ? Math.Min(Damage - 4, 5) : 0;
    public bool IsPowerDown => Status == RobotStatus.PowerDown;
    public bool NeedsReboot => Status == RobotStatus.Rebooting;

    public void SetPosition(Position newPos) => _position = newPos;
    public void Move(Position newPos) => _position = newPos;

    public void SetDirection(Direction direction) => _direction = direction;
    public void RotateLeft() => _direction = _direction.TurnLeft();
    public void RotateRight() => _direction = _direction.TurnRight();
    public void Rotate180() => _direction = _direction.TurnAround();

    public void AddDamage(int amount) => TakeDamage(amount);

    public void TakeDamage(int amount)
    {
        _damage += amount;
        if (_damage >= 10)
        {
            if (_lifeTokens > 0)
            {
                LoseLifeToken();
            }
            else
            {
                _status = RobotStatus.Destroyed;
            }
        }
    }

    public void AddEnergy(int amount) => _energy = Math.Min(_energy + amount, 10);
    public void SpendEnergy(int amount) => _energy = Math.Max(_energy - amount, 0);

    public void LoseLifeToken()
    {
        _lifeTokens--;
        _status = RobotStatus.Rebooting;
        _damage = 0;
    }

    public void Reboot(Position spawnPos)
    {
        _position = spawnPos;
        _direction = Direction.North;
        _status = RobotStatus.Active;
        _damage = 2;
        _checkpointsReached = 0;
    }

    public void SetStatus(RobotStatus newStatus) => _status = newStatus;
    public void PowerDown() => _status = RobotStatus.PowerDown;
    public void WakeUp() => _status = RobotStatus.Active;

    public void AddCheckpoint() => _checkpointsReached++;

    public void SetCheckpointsReached(int count) => _checkpointsReached = count;
}
