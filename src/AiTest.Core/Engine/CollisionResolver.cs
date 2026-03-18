namespace AiTest.Core.Engine;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public enum CollisionResult
{
    Success,
    Blocked,
    Pushed,
    Swap
}

public sealed class MoveIntent
{
    public required Robot Robot { get; init; }
    public Position From { get; init; }
    public Position To { get; init; }
    public Direction Direction { get; init; }
    public int Priority { get; init; }
    public int RegisterIndex { get; init; }
    public MoveType Type { get; init; }
}

public enum MoveType
{
    Normal,
    Push,
    Board
}

public sealed class CollisionResolver
{
    public List<MoveIntent> Intents { get; } = new();
    
    public void AddIntent(MoveIntent intent)
    {
        Intents.Add(intent);
    }
    
    public void Resolve(GameMap map, IReadOnlyList<Robot> robots)
    {
        var resolved = new HashSet<int>();
        
        foreach (var intent in Intents.OrderByDescending(i => i.Priority))
        {
            if (resolved.Contains(intent.Robot.Id))
                continue;
            
            if (resolved.Contains(intent.Robot.Id))
                continue;
                
            var result = TryMove(intent, map, robots, resolved);
            
            if (result != CollisionResult.Blocked)
                resolved.Add(intent.Robot.Id);
        }
    }
    
    private CollisionResult TryMove(MoveIntent intent, GameMap map, IReadOnlyList<Robot> robots, HashSet<int> resolved)
    {
        var targetRobot = robots.FirstOrDefault(r => 
            r.Id != intent.Robot.Id && 
            r.Position == intent.To &&
            r.IsAlive);
        
        if (targetRobot == null)
        {
            if (map.IsInBounds(intent.To))
            {
                var tile = map.GetTile(intent.To);
                if (tile.Type != TileType.Wall)
                {
                    intent.Robot.Move(intent.To);
                    return CollisionResult.Success;
                }
            }
            return CollisionResult.Blocked;
        }
        
        if (resolved.Contains(targetRobot.Id))
            return CollisionResult.Blocked;
        
        var pushIntent = new MoveIntent
        {
            Robot = targetRobot,
            From = targetRobot.Position,
            To = intent.To.Move(intent.Direction),
            Direction = intent.Direction,
            Priority = int.MinValue,
            RegisterIndex = intent.RegisterIndex,
            Type = MoveType.Push
        };
        
        var pushResult = TryMove(pushIntent, map, robots, resolved);
        
        if (pushResult == CollisionResult.Success)
        {
            intent.Robot.Move(intent.To);
            return CollisionResult.Pushed;
        }
        
        return CollisionResult.Blocked;
    }
}
