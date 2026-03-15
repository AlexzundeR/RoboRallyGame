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
    Checkpoint
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

    public static Tile Floor => new() { Type = TileType.Floor };
    public static Tile Wall => new() { Type = TileType.Wall };
    public static Tile Pit => new() { Type = TileType.Pit };
    public static Tile Start => new() { Type = TileType.Start };
    public static Tile LaserH => new() { Type = TileType.LaserH, HasBoardLaser = true };
    public static Tile LaserV => new() { Type = TileType.LaserV, HasBoardLaser = true };

    public static Tile Conveyor(ConveyorDirection dir, bool express = false) => new()
    {
        Type = express
            ? (dir is ConveyorDirection.North or ConveyorDirection.South ? TileType.ExpressConveyorCW : TileType.ExpressConveyorCCW)
            : (dir is ConveyorDirection.North or ConveyorDirection.South ? TileType.ConveyorCW : TileType.ConveyorCCW),
        ConveyorDir = dir
    };

    public static Tile Gear(bool clockwise = true) => new()
    {
        Type = clockwise ? TileType.GearCW : TileType.GearCCW
    };

    public static Tile Checkpoint(int number) => new()
    {
        Type = TileType.Checkpoint,
        CheckpointNumber = number
    };

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
            _ => '?'
        };
    }
}
