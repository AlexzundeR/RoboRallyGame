namespace AiTest.Core.Engine;

using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Upgrades;
using AiTest.Core.Upgrades;

public enum LaserPriorityMode
{
    None,
    ByPosition,
    ByDirection
}

public sealed class LaserShot
{
    public required Robot Source { get; init; }
    public Position From { get; init; }
    public Direction Direction { get; init; }
    public int Priority { get; init; }
    public bool IsRobotLaser { get; init; }
    public bool IsRearLaser { get; init; }
    public List<Position> Path { get; init; } = new();
}

public sealed class LaserResolver
{
    private readonly LaserPriorityMode _mode;
    
    public LaserResolver(LaserPriorityMode mode = LaserPriorityMode.ByPosition)
    {
        _mode = mode;
    }
    
    public List<LaserShot> PrepareShots(GameMap map, IReadOnlyList<Player> players)
    {
        var shots = new List<LaserShot>();
        
        foreach (var player in players)
        {
            var robot = player.Robot;
            if (!robot.IsAlive)
                continue;
            
            var laserStrategy = GetLaserStrategy(player);
            var fireDirections = laserStrategy.GetFireDirections(robot, robot.Direction).ToList();
            var fireRearLaser = laserStrategy.ShouldFireRearLaser(robot);
            var damageBonus = laserStrategy.GetDamageBonus(robot);
            
            var tile = map.GetTile(robot.Position);
            
            if (tile.Type == TileType.Laser)
            {
                shots.Add(new LaserShot
                {
                    Source = robot,
                    From = robot.Position,
                    Direction = GetBoardLaserDirection(tile),
                    Priority = GetPriority(robot, _mode),
                    IsRobotLaser = false,
                    Path = CalculatePath(robot.Position, GetBoardLaserDirection(tile), map)
                });
            }
            
            foreach (var dir in fireDirections)
            {
                shots.Add(new LaserShot
                {
                    Source = robot,
                    From = robot.Position,
                    Direction = dir,
                    Priority = GetPriority(robot, _mode),
                    IsRobotLaser = true,
                    Path = CalculatePath(robot.Position, dir, map)
                });
            }
            
            if (fireRearLaser)
            {
                var rearDir = robot.Direction.TurnAround();
                if (!fireDirections.Contains(rearDir))
                {
                    shots.Add(new LaserShot
                    {
                        Source = robot,
                        From = robot.Position,
                        Direction = rearDir,
                        Priority = GetPriority(robot, _mode),
                        IsRobotLaser = true,
                        IsRearLaser = true,
                        Path = CalculatePath(robot.Position, rearDir, map)
                    });
                }
            }
        }
        
        return shots;
    }
    
    private ILaserStrategy GetLaserStrategy(Player player)
    {
        var strategies = new List<ILaserStrategy> { DefaultLaserStrategy.Instance };
        
        foreach (var upgrade in player.AllUpgrades)
        {
            var strategy = upgrade.GetLaserStrategy();
            if (strategy != null)
                strategies.Add(strategy);
        }
        
        return new CompositeLaserStrategy(strategies);
    }
    
    public void Fire(List<LaserShot> shots, IReadOnlyList<Player> players)
    {
        var hits = new Dictionary<int, int>();
        
        var sortedShots = shots.OrderByDescending(s => s.Priority).ToList();
        
        foreach (var shot in sortedShots)
        {
            foreach (var pos in shot.Path)
            {
                var target = players.FirstOrDefault(p => 
                    p.Robot.Id != shot.Source.Id &&
                    p.Robot.IsAlive &&
                    p.Robot.Position == pos);
                
                if (target != null)
                {
                    hits.TryGetValue(target.Robot.Id, out var currentDamage);
                    hits[target.Robot.Id] = currentDamage + 1;
                }
            }
        }
        
        foreach (var (robotId, damage) in hits)
        {
            var player = players.FirstOrDefault(p => p.Robot.Id == robotId);
            player?.Robot.TakeDamage(damage);
        }
    }
    
    private Direction GetBoardLaserDirection(Tile tile)
    {
        return tile.LaserDirection ?? Direction.North;
    }
    
    private int GetPriority(Robot robot, LaserPriorityMode mode)
    {
        return mode switch
        {
            LaserPriorityMode.ByPosition => robot.Position.X * 100 + robot.Position.Y,
            LaserPriorityMode.ByDirection => (int)robot.Direction * 10,
            _ => 0
        };
    }
    
    private List<Position> CalculatePath(Position start, Direction dir, GameMap map)
    {
        var path = new List<Position>();
        var pos = start.Move(dir);
        
        while (map.IsInBounds(pos))
        {
            var tile = map.GetTile(pos);
            if (tile.Type == TileType.Wall)
                break;
            
            if (!tile.CanEnterFrom(dir))
                break;
                
            path.Add(pos);
            pos = pos.Move(dir);
        }
        
        return path;
    }
}
