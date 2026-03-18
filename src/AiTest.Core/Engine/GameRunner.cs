namespace AiTest.Core.Engine;

using AiTest.Core.Config;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public class GameRunner
{
    private readonly GameEngine _engine;
    private readonly GameConfiguration _config;

    public GameRunner(GameConfiguration config)
    {
        _config = config;
        var dealer = CreateDealer();
        _engine = new GameEngine(dealer);
    }

    public GameRunner(GameEngine engine)
    {
        _config = new GameConfiguration();
        _engine = engine;
    }

    public GameEngine Engine => _engine;
    public GameConfiguration Config => _config;

    public void Initialize()
    {
        var map = new GameMap(_config.MapWidth, _config.MapHeight);
        
        for (int x = 0; x < _config.MapWidth; x++)
        {
            for (int y = 0; y < _config.MapHeight; y++)
            {
                map.SetTile(x, y, Tile.Floor);
            }
        }

        foreach (var tile in _config.Map.Tiles)
        {
            map.SetTile(tile.X, tile.Y, tile.Type switch
            {
                TileType.Wall => Tile.Wall,
                TileType.Pit => Tile.Pit,
                TileType.Start => Tile.Start,
                TileType.LaserH => Tile.LaserH,
                TileType.LaserV => Tile.LaserV,
                _ => Tile.Floor
            });
        }

        foreach (var cp in _config.Map.Checkpoints)
        {
            map.SetTile(cp.X, cp.Y, Tile.Checkpoint(cp.Number));
        }

        foreach (var start in _config.Map.Starts)
        {
            map.SetTile(start.X, start.Y, Tile.Start);
        }

        _engine.Initialize(map);

        foreach (var playerConfig in _config.Players)
        {
            _engine.AddPlayer(playerConfig.Name);
        }

        _engine.StartGame();
    }

    private IDealer CreateDealer()
    {
        if (_config.Deck.CustomCards != null)
        {
            return new FixedDealer(_config.Deck.CustomCards);
        }

        if (_config.Deck.UseFullDeck)
        {
            return new RandomDealer();
        }

        return new RandomDealer();
    }

    public static GameConfiguration CreateDefault2Player()
    {
        return new GameConfiguration
        {
            MapWidth = 12,
            MapHeight = 12,
            Players = new List<PlayerConfig>
            {
                new() { Name = "Player1", StartPosition = new Position(1, 1) },
                new() { Name = "Player2", StartPosition = new Position(10, 1) }
            },
            Map = new MapConfig
            {
                Checkpoints = new List<CheckpointPlacement>
                {
                    new() { X = 5, Y = 5, Number = 1 },
                    new() { X = 6, Y = 5, Number = 2 },
                    new() { X = 7, Y = 5, Number = 3 },
                    new() { X = 8, Y = 5, Number = 4 }
                },
                Starts = new List<StartPlacement>
                {
                    new() { X = 1, Y = 1 },
                    new() { X = 10, Y = 1 },
                    new() { X = 1, Y = 10 },
                    new() { X = 10, Y = 10 }
                },
                Tiles = new List<TilePlacement>
                {
                    new() { X = 3, Y = 3, Type = TileType.Wall },
                    new() { X = 8, Y = 3, Type = TileType.Wall },
                    new() { X = 3, Y = 8, Type = TileType.Wall },
                    new() { X = 5, Y = 2, Type = TileType.Pit }
                }
            }
        };
    }

    public static GameConfiguration CreateMinimal()
    {
        return new GameConfiguration
        {
            MapWidth = 8,
            MapHeight = 8,
            Players = new List<PlayerConfig>
            {
                new() { Name = "Player1" }
            },
            Map = new MapConfig
            {
                Checkpoints = new List<CheckpointPlacement>
                {
                    new() { X = 3, Y = 3, Number = 1 }
                },
                Starts = new List<StartPlacement>
                {
                    new() { X = 1, Y = 1 }
                }
            }
        };
    }
}
