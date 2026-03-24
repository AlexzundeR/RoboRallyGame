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
        var resolvedRobots = new Dictionary<int, Position>();
        var robotsById = robots.ToDictionary(r => r.Id);
        var intentsByRobotId = Intents.ToDictionary(i => i.Robot.Id, i => i);
        
        foreach (var intent in Intents.OrderByDescending(i => i.Priority))
        {
            if (resolvedRobots.ContainsKey(intent.Robot.Id))
                continue;
            
            var result = TryMove(intent, map, robotsById, resolvedRobots, intentsByRobotId);
            
            if (result != CollisionResult.Blocked)
                resolvedRobots[intent.Robot.Id] = intent.Robot.Position;
        }
    }
    
    private CollisionResult TryMove(MoveIntent intent, GameMap map, Dictionary<int, Robot> robotsById, Dictionary<int, Position> resolvedRobots, Dictionary<int, MoveIntent> intentsByRobotId)
    {
        if (resolvedRobots.Values.Contains(intent.To))
            return CollisionResult.Blocked;
        
        var targetRobot = robotsById.Values.FirstOrDefault(r => 
            r.Id != intent.Robot.Id && 
            r.Position == intent.To &&
            r.IsAlive &&
            !resolvedRobots.ContainsKey(r.Id));
        
        if (targetRobot == null)
        {
            if (map.IsInBounds(intent.To))
            {
                var currentTile = map.GetTile(intent.Robot.Position);
                var targetTile = map.GetTile(intent.To);
                if (targetTile.CanEnterFrom(intent.Direction) && currentTile.CanExitTo(intent.Direction))
                {
                    intent.Robot.Move(intent.To);
                    return CollisionResult.Success;
                }
            }
            return CollisionResult.Blocked;
        }
        
        var targetHasIntent = intentsByRobotId.TryGetValue(targetRobot.Id, out var targetIntent);
        
        if (targetHasIntent)
        {
            if (intent.Priority > targetIntent.Priority)
            {
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
                
                var targetTileForPush = map.GetTile(intent.To);
                var pushTargetTile = map.GetTile(pushIntent.To);
                if (!pushTargetTile.CanEnterFrom(intent.Direction) || !targetTileForPush.CanExitTo(intent.Direction))
                {
                    return CollisionResult.Blocked;
                }
                
                var pushResult = TryMove(pushIntent, map, robotsById, resolvedRobots, intentsByRobotId);
                
                if (pushResult == CollisionResult.Success)
                {
                    intent.Robot.Move(intent.To);
                    return CollisionResult.Pushed;
                }
                
                return CollisionResult.Blocked;
            }
            
            return CollisionResult.Blocked;
        }
        
        var pushOnlyIntent = new MoveIntent
        {
            Robot = targetRobot,
            From = targetRobot.Position,
            To = intent.To.Move(intent.Direction),
            Direction = intent.Direction,
            Priority = int.MinValue,
            RegisterIndex = intent.RegisterIndex,
            Type = MoveType.Push
        };
        
        var tileAtTarget = map.GetTile(intent.To);
        var tileAtPushDest = map.GetTile(pushOnlyIntent.To);
        if (!tileAtPushDest.CanEnterFrom(intent.Direction) || !tileAtTarget.CanExitTo(intent.Direction))
        {
            return CollisionResult.Blocked;
        }
        
        if (map.IsInBounds(pushOnlyIntent.To))
        {
            targetRobot.Move(pushOnlyIntent.To);
            intent.Robot.Move(intent.To);
            return CollisionResult.Pushed;
        }
        
        return CollisionResult.Blocked;
    }
}
