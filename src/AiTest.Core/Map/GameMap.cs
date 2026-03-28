namespace AiTest.Core.Models;

using AiTest.Core.Models.Components;

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
            _tiles[x, y] = TileFactory.Floor();
    }

    public Tile GetTile(Position pos) => IsInBounds(pos) ? _tiles[pos.X, pos.Y] : TileFactory.Wall();
    public Tile GetTile(int x, int y) => IsInBounds(x, y) ? _tiles[x, y] : TileFactory.Wall();

    public void SetTile(Position pos, Tile tile)
    {
        if (IsInBounds(pos))
        {
            var tileWithPosition = new Tile(tile.Type, pos, tile.Components)
            {
                LaserDirection = tile.LaserDirection,
                CheckpointNumber = tile.CheckpointNumber
            };
            foreach (var component in tile.Components)
            {
                if (component is TileComponentBase tcb)
                    tcb.SetTile(tileWithPosition);
            }
            _tiles[pos.X, pos.Y] = tileWithPosition;
        }
    }

    public void SetTile(int x, int y, Tile tile) => SetTile(new Position(x, y), tile);

    public bool IsInBounds(Position pos) => IsInBounds(pos.X, pos.Y);
    public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public static GameMap CreateStandardMap()
    {
        var map = new GameMap(15, 15, "Factory Floor Test Map");

        for (int x = 0; x < 15; x++)
        {
            map.SetTile(x, 0, TileFactory.Wall());
            map.SetTile(x, 14, TileFactory.Wall());
        }
        for (int y = 0; y < 15; y++)
        {
            map.SetTile(0, y, TileFactory.Wall());
            map.SetTile(14, y, TileFactory.Wall());
        }

        // 4 Start positions
        map.SetTile(2, 2, TileFactory.Start());
        map.SetTile(12, 2, TileFactory.Start());
        map.SetTile(2, 12, TileFactory.Start());
        map.SetTile(12, 12, TileFactory.Start());

        // Center Checkpoints (4)
        map.SetTile(6, 6, TileFactory.Checkpoint(1));
        map.SetTile(7, 6, TileFactory.Checkpoint(2));
        map.SetTile(6, 7, TileFactory.Checkpoint(3));
        map.SetTile(7, 7, TileFactory.Checkpoint(4));

        // Conveyors - top
        map.SetTile(4, 3, TileFactory.Conveyor(ConveyorDirection.East));
        map.SetTile(5, 3, TileFactory.Conveyor(ConveyorDirection.East));
        map.SetTile(6, 3, TileFactory.Conveyor(ConveyorDirection.East));
        map.SetTile(9, 3, TileFactory.Conveyor(ConveyorDirection.West));
        map.SetTile(10, 3, TileFactory.Conveyor(ConveyorDirection.West));

        // Conveyors - bottom
        map.SetTile(4, 11, TileFactory.Conveyor(ConveyorDirection.East));
        map.SetTile(10, 11, TileFactory.Conveyor(ConveyorDirection.West));

        // Express Conveyors - left
        map.SetTile(3, 5, TileFactory.Conveyor(ConveyorDirection.South, true));
        map.SetTile(3, 6, TileFactory.Conveyor(ConveyorDirection.South, true));
        map.SetTile(3, 8, TileFactory.Conveyor(ConveyorDirection.North, true));
        map.SetTile(3, 9, TileFactory.Conveyor(ConveyorDirection.North, true));

        // Express Conveyors - right
        map.SetTile(11, 5, TileFactory.Conveyor(ConveyorDirection.South, true));
        map.SetTile(11, 6, TileFactory.Conveyor(ConveyorDirection.South, true));
        map.SetTile(11, 8, TileFactory.Conveyor(ConveyorDirection.North, true));
        map.SetTile(11, 9, TileFactory.Conveyor(ConveyorDirection.North, true));

        // Gears - top right
        map.SetTile(10, 4, TileFactory.Gear(true));
        map.SetTile(10, 5, TileFactory.Gear(true));

        // Gears - bottom left
        map.SetTile(4, 9, TileFactory.Gear(false));
        map.SetTile(4, 10, TileFactory.Gear(false));

        // Board Lasers
        map.SetTile(2, 5, TileFactory.Laser(Direction.West));
        map.SetTile(5, 2, TileFactory.Laser(Direction.North));
        map.SetTile(9, 2, TileFactory.Laser(Direction.North));
        map.SetTile(12, 5, TileFactory.Laser(Direction.East));
        map.SetTile(2, 9, TileFactory.Laser(Direction.West));
        map.SetTile(12, 9, TileFactory.Laser(Direction.East));

        // Pits
        map.SetTile(5, 12, TileFactory.Pit());
        map.SetTile(9, 12, TileFactory.Pit());

        // Repair Sites
        map.SetTile(7, 3, TileFactory.RepairSite());
        map.SetTile(7, 11, TileFactory.RepairSitePlus());

        // Barricade
        map.SetTile(7, 5, TileFactory.Barricade(Direction.North));

        // Tunnels
        map.SetTile(5, 9, TileFactory.TunnelEW());
        map.SetTile(9, 5, TileFactory.TunnelNS());

        return map;
    }
}
