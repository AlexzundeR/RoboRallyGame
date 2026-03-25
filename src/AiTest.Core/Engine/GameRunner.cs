namespace AiTest.Core.Engine;

using AiTest.Core.Config;
using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Models.Components;

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
            var coreTile = CreateTile(tile);
            map.SetTile(tile.X, tile.Y, coreTile);
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

    private static Core.Models.Tile CreateTile(TilePlacement placement)
    {
        var type = placement.Type switch
        {
            TileType.Wall => TileType.Wall,
            TileType.Pit => TileType.Pit,
            TileType.Start => TileType.Start,
            TileType.LaserH => TileType.LaserH,
            TileType.LaserV => TileType.LaserV,
            TileType.ConveyorCW => TileType.ConveyorCW,
            TileType.ConveyorCCW => TileType.ConveyorCCW,
            TileType.GearCW => TileType.GearCW,
            TileType.GearCCW => TileType.GearCCW,
            TileType.ExpressConveyorCW => TileType.ExpressConveyorCW,
            TileType.ExpressConveyorCCW => TileType.ExpressConveyorCCW,
            _ => TileType.Floor
        };

        var components = new List<ITileComponent>();
        
        if (placement.BlockedDirections != null && placement.BlockedDirections.Count > 0)
        {
            var blocked = DirectionSet.FromDirections(placement.BlockedDirections.ToArray());
            components.Add(new BlockedMovementComponent(blocked));
        }

        if (placement.ConveyorDir.HasValue)
        {
            var conveyorDir = placement.ConveyorDir.Value switch
            {
                Direction.North => ConveyorDirection.North,
                Direction.East => ConveyorDirection.East,
                Direction.South => ConveyorDirection.South,
                Direction.West => ConveyorDirection.West,
                _ => ConveyorDirection.North
            };
            
            if (components.Count > 0)
            {
                var tile = new Core.Models.Tile(type, components) { ConveyorDir = conveyorDir };
                return tile;
            }
            return new Core.Models.Tile(type) { ConveyorDir = conveyorDir };
        }

        return components.Count > 0 
            ? new Core.Models.Tile(type, components) 
            : new Core.Models.Tile(type);
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
}
