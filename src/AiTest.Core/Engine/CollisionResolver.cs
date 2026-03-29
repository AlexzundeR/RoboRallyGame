namespace AiTest.Core.Engine;

using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Models.Components;

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
    Board,
    Conveyor
}

public sealed class CollisionResolver
{
    public List<MoveIntent> Intents { get; } = new();
    
    public void AddIntent(MoveIntent intent) => Intents.Add(intent);
    
    public void Resolve(GameMap map, IReadOnlyList<Robot> robots)
    {
        var resolvedPositions = new Dictionary<int, Position>();
        var robotsById = robots.ToDictionary(r => r.Id);
        
        foreach (var intent in Intents.OrderByDescending(i => i.Priority))
        {
            if (resolvedPositions.ContainsKey(intent.Robot.Id))
                continue;
            
            var result = TryMove(intent, map, robotsById, resolvedPositions);
            
            if (result != CollisionResult.Blocked)
                resolvedPositions[intent.Robot.Id] = intent.Robot.Position;
        }
    }
    
    private static bool CanPassThrough(Position from, Position to, Direction dir, GameMap map)
    {
        if (!map.IsInBounds(to)) return false;
        
        var fromTile = map.GetTile(from);
        var toTile = map.GetTile(to);
        
        return toTile.CanEnterFrom(dir) && fromTile.CanExitTo(dir);
    }
    
    private CollisionResult TryMove(MoveIntent intent, GameMap map, Dictionary<int, Robot> robotsById, Dictionary<int, Position> resolvedPositions)
    {
        var from = intent.From;
        var to = intent.To;
        var dir = intent.Direction;
        var steps = Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y);
        
        for (int i = 1; i <= steps; i++)
        {
            var stepPos = from.Move(dir);
            
            if (!CanPassThrough(from, stepPos, dir, map))
                return CollisionResult.Blocked;
            
            var tile = map.GetTile(stepPos);
            var touchResult = tile.GetComponent<ITileBehaviorComponent>()?.ApplyOnTouch(intent.Robot, map);
            
            if (touchResult == TileActionResult.ReadyToFall)
            {
                intent.Robot.SetPosition(stepPos);
                return CollisionResult.Success;
            }
            
            if (resolvedPositions.ContainsValue(stepPos))
                return CollisionResult.Blocked;
            
            var robotOnTile = robotsById.Values.FirstOrDefault(r => 
                r.Id != intent.Robot.Id && 
                r.Position == stepPos &&
                r.IsAlive &&
                !resolvedPositions.ContainsKey(r.Id));
            
            if (robotOnTile != null)
            {
                if (i < steps) return CollisionResult.Blocked;
                
                var pushTarget = stepPos.Move(dir);
                if (!CanPassThrough(stepPos, pushTarget, dir, map))
                    return CollisionResult.Blocked;
                    
                robotOnTile.SetPosition(pushTarget);
                intent.Robot.SetPosition(stepPos);
                return CollisionResult.Pushed;
            }
            
            from = stepPos;
        }
        
        intent.Robot.SetPosition(to);
        return CollisionResult.Success;
    }
}
