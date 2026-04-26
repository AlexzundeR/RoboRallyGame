using RoboRallyGame.Core.Models.Components;

namespace RoboRallyGame.Core.Models;

public enum TileType
{
    Floor,
    Wall,
    Pit,
    Conveyor,
    Gear,
    Laser,
    Start,
    Checkpoint,
    RepairSite,
    RepairSitePlus
}

public enum ConveyorDirection
{
    North,
    East,
    South,
    West
}

public sealed class Tile
{
    public TileType Type { get; init; }
    public Direction? LaserDirection { get; init; }
    public int? CheckpointNumber { get; init; }
    public Position Position { get; init; }
    
    private readonly List<ITileComponent> _components;
    public IReadOnlyList<ITileComponent> Components => _components;

    public Tile() : this(TileType.Floor)
    {
    }

    public Tile(TileType type)
    {
        Type = type;
        _components = [];
    }

    public Tile(TileType type, IEnumerable<ITileComponent>? components) : this(type)
    {
        if (components == null) return;
        
        var componentList = components.ToList();
        var behaviors = componentList.OfType<ITileBehaviorComponent>().ToList();
        if (behaviors.Count > 1)
            throw new InvalidOperationException("Tile can have only one behavior component");
        
        foreach (var component in componentList)
            AddComponent(component);
    }

    public Tile(TileType type, Direction laserDirection) : this(type)
    {
        LaserDirection = laserDirection;
    }

    public Tile(TileType type, Position position) : this(type)
    {
        Position = position;
    }

    public Tile(TileType type, Position position, IEnumerable<ITileComponent>? components) 
        : this(type, components)
    {
        Position = position;
    }

    public void AddComponent(ITileComponent component)
    {
        if (component is ITileBehaviorComponent)
        {
            var existingBehavior = _components.OfType<ITileBehaviorComponent>().FirstOrDefault();
            if (existingBehavior != null)
                throw new InvalidOperationException("Tile can have only one behavior component");
            
            if (component is TileComponentBase tcb)
                tcb.SetTile(this);
        }
        
        _components.Add(component);
    }

    public T? GetComponent<T>() where T : class, ITileComponent
    {
        foreach (var component in _components)
            if (component is T typed)
                return typed;
        return null;
    }

    public char ToChar()
    {
        if (Type == TileType.Laser)
            return LaserDirection is Direction.East or Direction.West ? '━' : '┃';
        
        if (Type == TileType.Conveyor)
        {
            var conveyor = GetComponent<ConveyorBehaviorComponent>();
            return conveyor?.IsExpress == true ? '⤴' : '↻';
        }
        
        if (Type == TileType.Gear)
        {
            var gear = GetComponent<GearBehaviorComponent>();
            return gear?.IsClockwise == true ? '↻' : '↺';
        }
        
        return Type switch
        {
            TileType.Floor => '·',
            TileType.Wall => '█',
            TileType.Pit => ' ',
            TileType.Start => 'S',
            TileType.Checkpoint => CheckpointNumber.HasValue ? CheckpointNumber.Value.ToString()[0] : 'C',
            TileType.RepairSite => '⚙',
            TileType.RepairSitePlus => '⚒',
            _ => '?'
        };
    }
}
