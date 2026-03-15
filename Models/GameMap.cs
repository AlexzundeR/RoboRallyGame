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
        var map = new GameMap(12, 12, "Factory Floor");

        for (int x = 0; x < 12; x++)
        {
            map.SetTile(x, 0, Tile.Wall);
            map.SetTile(x, 11, Tile.Wall);
        }
        for (int y = 0; y < 12; y++)
        {
            map.SetTile(0, y, Tile.Wall);
            map.SetTile(11, y, Tile.Wall);
        }

        map.SetTile(2, 2, Tile.Start);
        map.SetTile(9, 2, Tile.Start);
        map.SetTile(2, 9, Tile.Start);
        map.SetTile(9, 9, Tile.Start);

        map.SetTile(5, 5, Tile.Checkpoint(1));
        map.SetTile(6, 5, Tile.Checkpoint(2));
        map.SetTile(5, 6, Tile.Checkpoint(3));
        map.SetTile(6, 6, Tile.Checkpoint(4));

        map.SetTile(3, 3, Tile.Conveyor(ConveyorDirection.East));
        map.SetTile(4, 3, Tile.Conveyor(ConveyorDirection.East));
        map.SetTile(8, 3, Tile.Conveyor(ConveyorDirection.West));

        map.SetTile(3, 8, Tile.Gear(true));
        map.SetTile(8, 8, Tile.Gear(false));

        map.SetTile(2, 5, Tile.LaserH);
        map.SetTile(9, 5, Tile.LaserV);

        map.SetTile(5, 2, Tile.Pit);
        map.SetTile(6, 9, Tile.Pit);

        return map;
    }
}
