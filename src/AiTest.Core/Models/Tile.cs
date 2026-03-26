using AiTest.Core.Models.Components;

namespace AiTest.Core.Models;

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
    public int? CheckpointNumber { get; set; }
    
    private readonly IReadOnlyList<ITileComponent> _components;
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
        var componentList = components?.ToList() ?? [];
        var behaviors = componentList.OfType<Components.ITileBehaviorComponent>().ToList();
        if (behaviors.Count > 1)
            throw new InvalidOperationException("Tile can have only one behavior component");
        _components = componentList;
    }

    public Tile(TileType type, Direction laserDirection) : this(type)
    {
        LaserDirection = laserDirection;
    }

    public T? GetComponent<T>() where T : class, ITileComponent
    {
        foreach (var component in _components)
            if (component is T typed)
                return typed;
        return null;
    }

    public static Tile Floor => new(TileType.Floor);
    public static Tile Wall => new(TileType.Wall);
    public static Tile Pit => new(TileType.Pit);
    public static Tile Start => new(TileType.Start);

    public static Tile Laser(Direction direction) => new(TileType.Laser) { LaserDirection = direction };

    public static Tile Conveyor(ConveyorDirection dir, bool express = false) 
    {
        return new Tile(TileType.Conveyor, [new ConveyorBehaviorComponent(dir, express)]);
    }

    public static Tile Gear(bool clockwise = true) 
        => new Tile(TileType.Gear, [new Components.GearBehaviorComponent(clockwise)]);

    public static Tile Checkpoint(int number) => new(TileType.Checkpoint)
    {
        CheckpointNumber = number
    };

    public static Tile RepairSite => new(TileType.RepairSite);
    public static Tile RepairSitePlus => new(TileType.RepairSitePlus);

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
