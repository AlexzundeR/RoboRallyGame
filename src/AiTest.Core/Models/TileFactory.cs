using AiTest.Core.Models.Components;

namespace AiTest.Core.Models;

public static class TileFactory
{
    public static Tile Floor() => new(TileType.Floor);

    public static Tile Floor(params Direction[] blockedDirections) => new(TileType.Floor, [Blocked(blockedDirections)]);

    public static Tile Wall() => new(TileType.Wall);

    public static Tile Pit() => new(TileType.Pit);

    public static Tile Pit(params Direction[] blockedDirections) => new(TileType.Pit, [Blocked(blockedDirections)]);

    public static Tile Start() => new(TileType.Start);

    public static Tile Laser(Direction direction) 
    {
        var component = new LaserBehaviorComponent(direction);
        return new Tile(TileType.Laser, [component]) { LaserDirection = direction };
    }

    public static Tile Conveyor(ConveyorDirection dir, bool express = false)
    {
        var component = new ConveyorBehaviorComponent(dir, express);
        return new Tile(TileType.Conveyor, [component]);
    }

    public static Tile Conveyor(ConveyorDirection dir, bool express, params Direction[] blockedDirections)
    {
        var component = new ConveyorBehaviorComponent(dir, express);
        return new Tile(TileType.Conveyor, [component, Blocked(blockedDirections)]);
    }

    public static Tile Gear(bool clockwise = true) 
    {
        var component = new GearBehaviorComponent(clockwise);
        return new Tile(TileType.Gear, [component]);
    }

    public static Tile Checkpoint(int number) 
    {
        var component = new CheckpointBehaviorComponent(number);
        return new Tile(TileType.Checkpoint, [component]) { CheckpointNumber = number };
    }

    public static Tile TunnelNS() => new(TileType.Floor, [new BlockedMovementComponent(DirectionSet.FromDirections(Direction.East, Direction.West))]);

    public static Tile TunnelEW() => new(TileType.Floor, [new BlockedMovementComponent(DirectionSet.FromDirections(Direction.North, Direction.South))]);

    public static Tile Barricade(Direction openDirection) => new(TileType.Floor, [Blocked(AllExcept(openDirection))]);

    public static Tile Barricade(params Direction[] openDirections) => new(TileType.Floor, [Blocked(AllExcept(openDirections))]);

    public static Tile Barricade(DirectionSet openDirections) => new(TileType.Floor, [new BlockedMovementComponent(DirectionSet.All.Except(openDirections))]);

    public static Tile RepairSite() => new(TileType.RepairSite);

    public static Tile RepairSitePlus() => new(TileType.RepairSitePlus);

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
