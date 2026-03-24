using AiTest.Core.Models.Components;

namespace AiTest.Core.Models;

public enum TileType
{
    Floor,
    Wall,
    Pit,
    ConveyorCW,
    ConveyorCCW,
    GearCW,
    GearCCW,
    LaserH,
    LaserV,
    ExpressConveyorCW,
    ExpressConveyorCCW,
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
    public ConveyorDirection? ConveyorDir { get; init; }
    public bool HasBoardLaser { get; init; }
    public int? CheckpointNumber { get; set; }
    
    private readonly IReadOnlyList<ITileComponent> _components;
    public IReadOnlyList<ITileComponent> Components => _components;

    public Tile() : this(TileType.Floor, [])
    {
    }

    public Tile(TileType type, IEnumerable<ITileComponent>? components = null)
    {
        Type = type;
        _components = components?.ToList() ?? [];
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
    public static Tile LaserH => new(TileType.LaserH) { HasBoardLaser = true };
    public static Tile LaserV => new(TileType.LaserV) { HasBoardLaser = true };

    public static Tile Conveyor(ConveyorDirection dir, bool express = false) => new(express
        ? (dir is ConveyorDirection.North or ConveyorDirection.South ? TileType.ExpressConveyorCW : TileType.ExpressConveyorCCW)
        : (dir is ConveyorDirection.North or ConveyorDirection.South ? TileType.ConveyorCW : TileType.ConveyorCCW))
    {
        ConveyorDir = dir
    };

    public static Tile Gear(bool clockwise = true) => new(clockwise ? TileType.GearCW : TileType.GearCCW);

    public static Tile Checkpoint(int number) => new(TileType.Checkpoint)
    {
        CheckpointNumber = number
    };

    public static Tile RepairSite => new(TileType.RepairSite);
    public static Tile RepairSitePlus => new(TileType.RepairSitePlus);

    public char ToChar()
    {
        if (HasBoardLaser)
            return Type == TileType.LaserH ? '━' : '┃';
        
        return Type switch
        {
            TileType.Floor => '·',
            TileType.Wall => '█',
            TileType.Pit => ' ',
            TileType.ConveyorCW => '↻',
            TileType.ConveyorCCW => '↺',
            TileType.ExpressConveyorCW => '⤴',
            TileType.ExpressConveyorCCW => '⤵',
            TileType.GearCW => '↺',
            TileType.GearCCW => '↻',
            TileType.Start => 'S',
            TileType.Checkpoint => CheckpointNumber.HasValue ? CheckpointNumber.Value.ToString()[0] : 'C',
            TileType.RepairSite => '⚙',
            TileType.RepairSitePlus => '⚒',
            _ => '?'
        };
    }
}
