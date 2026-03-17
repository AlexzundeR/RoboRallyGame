namespace AiTest.Core.Upgrades;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public enum UpgradeType
{
    Permanent,
    Temporary
}

public enum TriggerPhase
{
    BeforeCard,
    AfterCard,
    BeforeMovement,
    AfterMovement,
    BeforeLaser,
    AfterLaser,
    OnDamage,
    OnHit,
    OnMove
}

public abstract class UpgradeEffect
{
    public abstract string Name { get; }
    public abstract UpgradeType Type { get; }
    
    public virtual IMovementStrategy? GetMovementStrategy() => null;
    public virtual ILaserStrategy? GetLaserStrategy() => null;
    public virtual IDamageStrategy? GetDamageStrategy() => null;
    public virtual IConveyorStrategy? GetConveyorStrategy() => null;
    public virtual ICardExecutionStrategy? GetCardExecutionStrategy() => null;
}

public interface ICardExecutionStrategy
{
    void ExecuteRotation(Robot robot);
    int GetExtraMoveSpaces(Robot robot, int baseSpaces);
    bool ShouldRepeatLastRegister(Robot robot);
}

public class UpgradeContext
{
    public required Robot Robot { get; init; }
    public GameMap? Map { get; init; }
    public IGameEngine? Engine { get; init; }
    public ProgramCard? CurrentCard { get; init; }
    public int RegisterIndex { get; init; }
    public Position? FromPosition { get; init; }
    public Position? ToPosition { get; init; }
    public List<Robot> AffectedRobots { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public interface IGameEngine
{
    // Placeholder - actual implementation in Engine
}

public static class UpgradeRegistry
{
    private static readonly Dictionary<string, Func<UpgradeEffect>> _factories = new();
    
    public static void Register(string name, Func<UpgradeEffect> factory)
    {
        _factories[name] = factory;
    }
    
    public static UpgradeEffect? Create(string name)
    {
        return _factories.TryGetValue(name, out var factory) ? factory() : null;
    }
    
    public static IReadOnlyDictionary<string, Func<UpgradeEffect>> All => _factories;
}

public interface IMovementStrategy
{
    Position CalculateMove(Robot robot, GameMap map, Position target);
    bool CanEnterTile(Robot robot, GameMap map, Position pos);
    bool ShouldStopAtObstacle(Robot robot);
    int GetExtraMoveSpaces(Robot robot);
}

public interface ILaserStrategy
{
    IEnumerable<Direction> GetFireDirections(Robot robot, Direction defaultDirection);
    int GetDamageBonus(Robot robot);
    bool ShouldFireRearLaser(Robot robot);
}

public interface IDamageStrategy
{
    int CalculateDamage(Robot robot, int baseDamage);
    bool CanDeflect(Robot robot);
    bool CanBounceBack(Robot robot, out int bounceDistance);
    int GetCollisionDamage(Robot robot);
}

public interface IConveyorStrategy
{
    bool ShouldIgnoreConveyors(Robot robot);
    bool ShouldIgnorePits(Robot robot);
}

public sealed class DefaultMovementStrategy : IMovementStrategy
{
    public static readonly DefaultMovementStrategy Instance = new();
    
    public Position CalculateMove(Robot robot, GameMap map, Position target) => target;
    
    public bool CanEnterTile(Robot robot, GameMap map, Position pos)
    {
        if (!map.IsInBounds(pos)) return false;
        var tile = map.GetTile(pos);
        return tile.Type != TileType.Wall;
    }
    
    public bool ShouldStopAtObstacle(Robot robot) => false;
    
    public int GetExtraMoveSpaces(Robot robot) => 0;
}

public sealed class DefaultLaserStrategy : ILaserStrategy
{
    public static readonly DefaultLaserStrategy Instance = new();
    
    public IEnumerable<Direction> GetFireDirections(Robot robot, Direction defaultDirection)
    {
        return new[] { defaultDirection };
    }
    
    public int GetDamageBonus(Robot robot) => 0;
    
    public bool ShouldFireRearLaser(Robot robot) => false;
}

public sealed class DefaultDamageStrategy : IDamageStrategy
{
    public static readonly DefaultDamageStrategy Instance = new();
    
    public int CalculateDamage(Robot robot, int baseDamage) => baseDamage;
    
    public bool CanDeflect(Robot robot) => false;
    
    public bool CanBounceBack(Robot robot, out int bounceDistance)
    {
        bounceDistance = 0;
        return false;
    }
    
    public int GetCollisionDamage(Robot robot) => 0;
}

public sealed class DefaultConveyorStrategy : IConveyorStrategy
{
    public static readonly DefaultConveyorStrategy Instance = new();
    
    public bool ShouldIgnoreConveyors(Robot robot) => false;
    public bool ShouldIgnorePits(Robot robot) => false;
}

public sealed class DefaultCardExecutionStrategy : ICardExecutionStrategy
{
    public static readonly DefaultCardExecutionStrategy Instance = new();
    
    public void ExecuteRotation(Robot robot) { }
    
    public int GetExtraMoveSpaces(Robot robot, int baseSpaces) => 0;
    
    public bool ShouldRepeatLastRegister(Robot robot) => false;
}

public sealed class CompositeMovementStrategy : IMovementStrategy
{
    private readonly List<IMovementStrategy> _strategies;

    public CompositeMovementStrategy(IEnumerable<IMovementStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public Position CalculateMove(Robot robot, GameMap map, Position target)
    {
        var result = target;
        foreach (var strategy in _strategies)
        {
            result = strategy.CalculateMove(robot, map, result);
        }
        return result;
    }

    public bool CanEnterTile(Robot robot, GameMap map, Position pos)
    {
        return _strategies.All(s => s.CanEnterTile(robot, map, pos));
    }

    public bool ShouldStopAtObstacle(Robot robot)
    {
        return _strategies.Any(s => s.ShouldStopAtObstacle(robot));
    }

    public int GetExtraMoveSpaces(Robot robot)
    {
        return _strategies.Sum(s => s.GetExtraMoveSpaces(robot));
    }
}

public sealed class CompositeLaserStrategy : ILaserStrategy
{
    private readonly List<ILaserStrategy> _strategies;

    public CompositeLaserStrategy(IEnumerable<ILaserStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public IEnumerable<Direction> GetFireDirections(Robot robot, Direction defaultDirection)
    {
        var directions = new HashSet<Direction> { defaultDirection };
        foreach (var strategy in _strategies)
        {
            directions.UnionWith(strategy.GetFireDirections(robot, defaultDirection));
        }
        return directions;
    }

    public int GetDamageBonus(Robot robot)
    {
        return _strategies.Sum(s => s.GetDamageBonus(robot));
    }

    public bool ShouldFireRearLaser(Robot robot)
    {
        return _strategies.Any(s => s.ShouldFireRearLaser(robot));
    }
}

public sealed class CompositeDamageStrategy : IDamageStrategy
{
    private readonly List<IDamageStrategy> _strategies;

    public CompositeDamageStrategy(IEnumerable<IDamageStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public int CalculateDamage(Robot robot, int baseDamage)
    {
        var damage = baseDamage;
        foreach (var strategy in _strategies)
        {
            damage = strategy.CalculateDamage(robot, damage);
        }
        return damage;
    }

    public bool CanDeflect(Robot robot)
    {
        return _strategies.Any(s => s.CanDeflect(robot));
    }

    public bool CanBounceBack(Robot robot, out int bounceDistance)
    {
        bounceDistance = 0;
        foreach (var strategy in _strategies)
        {
            if (strategy.CanBounceBack(robot, out var distance))
            {
                bounceDistance = Math.Max(bounceDistance, distance);
            }
        }
        return bounceDistance > 0;
    }

    public int GetCollisionDamage(Robot robot)
    {
        return _strategies.Sum(s => s.GetCollisionDamage(robot));
    }
}

public sealed class CompositeConveyorStrategy : IConveyorStrategy
{
    private readonly List<IConveyorStrategy> _strategies;

    public CompositeConveyorStrategy(IEnumerable<IConveyorStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public bool ShouldIgnoreConveyors(Robot robot)
    {
        return _strategies.Any(s => s.ShouldIgnoreConveyors(robot));
    }

    public bool ShouldIgnorePits(Robot robot)
    {
        return _strategies.Any(s => s.ShouldIgnorePits(robot));
    }
}

public sealed class CompositeCardExecutionStrategy : ICardExecutionStrategy
{
    private readonly List<ICardExecutionStrategy> _strategies;

    public CompositeCardExecutionStrategy(IEnumerable<ICardExecutionStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public void ExecuteRotation(Robot robot)
    {
        foreach (var strategy in _strategies)
        {
            strategy.ExecuteRotation(robot);
        }
    }

    public int GetExtraMoveSpaces(Robot robot, int baseSpaces)
    {
        return _strategies.Sum(s => s.GetExtraMoveSpaces(robot, baseSpaces));
    }

    public bool ShouldRepeatLastRegister(Robot robot)
    {
        return _strategies.Any(s => s.ShouldRepeatLastRegister(robot));
    }
}
