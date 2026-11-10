namespace RoboRallyGame.Godot.Models;

using System.Collections.Generic;

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
    Barricade
}

public enum ConveyorDirection
{
    North,
    East,
    South,
    West
}

public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public sealed record TileData(
    int X, 
    int Y, 
    TileType Type, 
    int? CheckpointNumber = null,
    ConveyorDirection? ConveyorDir = null,
    bool? IsExpress = null,
    bool? IsClockwise = null,
    IReadOnlyList<int>? BlockedDirections = null
);

public sealed record MapData(int Width, int Height, TileData[,] Tiles, int TotalCheckpoints)
{
    public TileData GetTile(int x, int y) => Tiles[x, y];
}