namespace RoboRallyGame.Core.Engine;

using RoboRallyGame.Core.Config;
using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Core.Models.Components;

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
                map.SetTile(x, y, TileFactory.Floor());
            }
        }

        foreach (var tile in _config.Map.Tiles)
        {
            var coreTile = CreateTile(tile);
            map.SetTile(tile.X, tile.Y, coreTile);
        }

        foreach (var cp in _config.Map.Checkpoints)
        {
            map.SetTile(cp.X, cp.Y, TileFactory.Checkpoint(cp.Number));
        }

        foreach (var start in _config.Map.Starts)
        {
            map.SetTile(start.X, start.Y, TileFactory.Start());
        }

        _engine.Initialize(map);

        foreach (var playerConfig in _config.Players)
        {
            _engine.AddPlayer(playerConfig.Name, playerConfig.StartPosition, playerConfig.StartDirection);
        }

        _engine.StartGame();
    }

    private static Tile CreateTile(TilePlacement placement)
    {
        var type = placement.Type;

        List<ITileComponent>? components = null;
        
        if (placement.BlockedDirections != null && placement.BlockedDirections.Count > 0)
        {
            components = [new BlockedMovementComponent(DirectionSet.FromDirections(placement.BlockedDirections.ToArray()))];
        }

        if (type == TileType.Conveyor && placement.ConveyorDir.HasValue)
        {
            var conveyorDir = placement.ConveyorDir.Value switch
            {
                Direction.North => ConveyorDirection.North,
                Direction.East => ConveyorDirection.East,
                Direction.South => ConveyorDirection.South,
                Direction.West => ConveyorDirection.West,
                _ => ConveyorDirection.North
            };
            
            var tile = TileFactory.Conveyor(conveyorDir, placement.IsExpress ?? false);
            
            if (placement.BlockedDirections != null && placement.BlockedDirections.Count > 0)
            {
                var behavior = tile.GetComponent<ConveyorBehaviorComponent>();
                return new Tile(type, [behavior!, new BlockedMovementComponent(DirectionSet.FromDirections(placement.BlockedDirections.ToArray()))]);
            }
            
            return tile;
        }

        if (type == TileType.Gear)
        {
            return TileFactory.Gear(placement.IsClockwise ?? true);
        }

        if (type == TileType.Laser && placement.LaserDirection.HasValue)
        {
            return new Tile(type, placement.LaserDirection.Value);
        }

        if (type == TileType.Pit)
        {
            return TileFactory.Pit();
        }

        if (type == TileType.RepairSite)
        {
            return TileFactory.RepairSite();
        }

        if (type == TileType.RepairSitePlus)
        {
            return TileFactory.RepairSitePlus();
        }

        return components != null 
            ? new Tile(type, components) 
            : new Tile(type);
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
