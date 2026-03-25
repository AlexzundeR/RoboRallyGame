namespace AiTest.Core.Models;

public sealed class GameMap
{
    private readonly Tile[,] _tiles;
    public int Width { get; }
    public int Height { get; }
    public string Name { get; }

    public GameMap(int width, int height, string name = "Unnamed")
    {
        Width = width;
        Height = height;
        Name = name;
        _tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            _tiles[x, y] = Tile.Floor;
    }

    public Tile GetTile(Position pos) => IsInBounds(pos) ? _tiles[pos.X, pos.Y] : Tile.Wall;
    public Tile GetTile(int x, int y) => IsInBounds(x, y) ? _tiles[x, y] : Tile.Wall;

    public void SetTile(Position pos, Tile tile)
    {
        if (IsInBounds(pos))
            _tiles[pos.X, pos.Y] = tile;
    }

    public void SetTile(int x, int y, Tile tile)
    {
        if (IsInBounds(x, y))
            _tiles[x, y] = tile;
    }

    public bool IsInBounds(Position pos) => IsInBounds(pos.X, pos.Y);
    public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public static GameMap CreateStandardMap()
    {
        var map = new GameMap(15, 15, "Factory Floor Test Map");

        for (int x = 0; x < 15; x++)
        {
            map.SetTile(x, 0, Tile.Wall);
            map.SetTile(x, 14, Tile.Wall);
        }
        for (int y = 0; y < 15; y++)
        {
            map.SetTile(0, y, Tile.Wall);
            map.SetTile(14, y, Tile.Wall);
        }

        // 4 Start positions
        map.SetTile(2, 2, Tile.Start);
        map.SetTile(12, 2, Tile.Start);
        map.SetTile(2, 12, Tile.Start);
        map.SetTile(12, 12, Tile.Start);

        // Center Checkpoints (4)
        map.SetTile(6, 6, Tile.Checkpoint(1));
        map.SetTile(7, 6, Tile.Checkpoint(2));
        map.SetTile(6, 7, Tile.Checkpoint(3));
        map.SetTile(7, 7, Tile.Checkpoint(4));

        // Conveyors - top
        map.SetTile(4, 3, Tile.Conveyor(ConveyorDirection.East));
        map.SetTile(5, 3, Tile.Conveyor(ConveyorDirection.East));
        map.SetTile(6, 3, Tile.Conveyor(ConveyorDirection.East));
        map.SetTile(9, 3, Tile.Conveyor(ConveyorDirection.West));
        map.SetTile(10, 3, Tile.Conveyor(ConveyorDirection.West));

        // Conveyors - bottom
        map.SetTile(4, 11, Tile.Conveyor(ConveyorDirection.East));
        map.SetTile(10, 11, Tile.Conveyor(ConveyorDirection.West));

        // Express Conveyors - left
        map.SetTile(3, 5, Tile.Conveyor(ConveyorDirection.South, true));
        map.SetTile(3, 6, Tile.Conveyor(ConveyorDirection.South, true));
        map.SetTile(3, 8, Tile.Conveyor(ConveyorDirection.North, true));
        map.SetTile(3, 9, Tile.Conveyor(ConveyorDirection.North, true));

        // Express Conveyors - right
        map.SetTile(11, 5, Tile.Conveyor(ConveyorDirection.South, true));
        map.SetTile(11, 6, Tile.Conveyor(ConveyorDirection.South, true));
        map.SetTile(11, 8, Tile.Conveyor(ConveyorDirection.North, true));
        map.SetTile(11, 9, Tile.Conveyor(ConveyorDirection.North, true));

        // Gears - top right
        map.SetTile(10, 4, Tile.Gear(true));
        map.SetTile(10, 5, Tile.Gear(true));

        // Gears - bottom left
        map.SetTile(4, 9, Tile.Gear(false));
        map.SetTile(4, 10, Tile.Gear(false));

        // Board Lasers
        map.SetTile(2, 5, Tile.Laser(Direction.West));
        map.SetTile(5, 2, Tile.Laser(Direction.North));
        map.SetTile(9, 2, Tile.Laser(Direction.North));
        map.SetTile(12, 5, Tile.Laser(Direction.East));
        map.SetTile(2, 9, Tile.Laser(Direction.West));
        map.SetTile(12, 9, Tile.Laser(Direction.East));

        // Pits
        map.SetTile(5, 12, Tile.Pit);
        map.SetTile(9, 12, Tile.Pit);

        // Repair Sites
        map.SetTile(7, 3, Tile.RepairSite);
        map.SetTile(7, 11, Tile.RepairSitePlus);

        // Barricade
        map.SetTile(7, 5, TileFactory.Barricade(Direction.North));

        // Tunnels
        map.SetTile(5, 9, TileFactory.TunnelEW());
        map.SetTile(9, 5, TileFactory.TunnelNS());

        return map;
    }
}
