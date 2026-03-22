using AiTest.Core.Models.Components;

namespace AiTest.Core.Models;

public static class TileFactory
{
    public static Tile Floor() => new(TileType.Floor);

    public static Tile Floor(params Direction[] blockedDirections) => new(TileType.Floor, [Blocked(blockedDirections)]);

    public static Tile Wall() => new(TileType.Wall);

    public static Tile Wall(params Direction[] blockedDirections) => new(TileType.Wall, [Blocked(blockedDirections)]);

    public static Tile Wall(DirectionSet blockedDirections) => new(TileType.Wall, [new BlockedMovementComponent(blockedDirections)]);

    public static Tile Pit() => new(TileType.Pit);

    public static Tile Pit(params Direction[] blockedDirections) => new(TileType.Pit, [Blocked(blockedDirections)]);

    public static Tile Start() => new(TileType.Start);

    public static Tile LaserH() => new(TileType.LaserH) { HasBoardLaser = true };

    public static Tile LaserV() => new(TileType.LaserV) { HasBoardLaser = true };

    public static Tile Conveyor(ConveyorDirection dir, bool express = false)
    {
        var type = express
            ? (dir is ConveyorDirection.North or ConveyorDirection.South ? TileType.ExpressConveyorCW : TileType.ExpressConveyorCCW)
            : (dir is ConveyorDirection.North or ConveyorDirection.South ? TileType.ConveyorCW : TileType.ConveyorCCW);

        return new Tile(type) { ConveyorDir = dir };
    }

    public static Tile Conveyor(ConveyorDirection dir, bool express, params Direction[] blockedDirections)
        => new(typeFromConveyor(dir, express), [Blocked(blockedDirections)]) { ConveyorDir = dir };

    public static Tile Gear(bool clockwise = true) => new(clockwise ? TileType.GearCW : TileType.GearCCW);

    public static Tile Checkpoint(int number) => new(TileType.Checkpoint) { CheckpointNumber = number };

    public static Tile TunnelNS() => new(TileType.Wall, [new BlockedMovementComponent(DirectionSet.FromDirections(Direction.East, Direction.West))]);

    public static Tile TunnelEW() => new(TileType.Wall, [new BlockedMovementComponent(DirectionSet.FromDirections(Direction.North, Direction.South))]);

    public static Tile Barricade(Direction openDirection) => new(TileType.Wall, [Blocked(AllExcept(openDirection))]);

    public static Tile Barricade(params Direction[] openDirections) => new(TileType.Wall, [Blocked(AllExcept(openDirections))]);

    public static Tile Barricade(DirectionSet openDirections) => new(TileType.Wall, [new BlockedMovementComponent(DirectionSet.All.Except(openDirections))]);

    private static TileType typeFromConveyor(ConveyorDirection dir, bool express) => express
        ? (dir is ConveyorDirection.North or ConveyorDirection.South ? TileType.ExpressConveyorCW : TileType.ExpressConveyorCCW)
        : (dir is ConveyorDirection.North or ConveyorDirection.South ? TileType.ConveyorCW : TileType.ConveyorCCW);

    private static BlockedMovementComponent Blocked(params Direction[] directions) 
        => new(DirectionSet.FromDirections(directions));

    private static BlockedMovementComponent Blocked(DirectionSet directions) 
        => new(directions);

    private static DirectionSet AllExcept(params Direction[] allowed)
    {
        DirectionSet blocked = DirectionSet.All;
        foreach (var dir in allowed)
            blocked = blocked.Remove(dir);
        return blocked;
    }
}
