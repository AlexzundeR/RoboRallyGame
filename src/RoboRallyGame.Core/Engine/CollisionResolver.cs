namespace RoboRallyGame.Core.Engine;

using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Core.Models.Components;

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
    public required Position From { get; init; }
    public required Position To { get; init; }
    public required Direction Direction { get; init; }
    public int Priority { get; init; }
    public int RegisterIndex { get; init; }
    public MoveType Type { get; init; }
}

public enum MoveType
{
    Normal,
    Push,
    Board,
    Conveyor,
    Rotation
}

public sealed record RobotMovement(int RobotId, Position From, Position To, CollisionResult Result);

public sealed record CollisionResolutionResult(IReadOnlyList<RobotMovement> Movements);

public sealed class CollisionResolver
{
    public List<MoveIntent> Intents { get; } = new();
    
    public void AddIntent(MoveIntent intent) => Intents.Add(intent);
    
    public CollisionResolutionResult Resolve(GameMap map, IReadOnlyList<Robot> robots)
    {
        var movements = new List<RobotMovement>();
        var resolvedPositions = new Dictionary<int, Position>();
        var robotsById = robots.ToDictionary(r => r.Id);
        
        foreach (var intent in Intents.OrderByDescending(i => i.Priority))
        {
            if (resolvedPositions.ContainsKey(intent.Robot.Id))
                continue;
            
            var result = TryMove(intent, map, robotsById, resolvedPositions);
            
            if (result.Result != CollisionResult.Blocked)
            {
                resolvedPositions[intent.Robot.Id] = intent.Robot.Position;
                movements.Add(result);
            }
        }
        
        return new CollisionResolutionResult(movements);
    }
    
    private static bool CanPassThrough(Position from, Position to, Direction dir, GameMap map)
    {
        if (!map.IsInBounds(to)) return false;
        
        var fromTile = map.GetTile(from);
        var toTile = map.GetTile(to);
        var entrySide = dir.TurnAround();
        
        return toTile.CanEnterFrom(entrySide) && fromTile.CanExitTo(dir);
    }
    
    private RobotMovement TryMove(MoveIntent intent, GameMap map, Dictionary<int, Robot> robotsById, Dictionary<int, Position> resolvedPositions)
    {
        if (intent.Type == MoveType.Rotation)
        {
            intent.Robot.SetDirection(intent.Direction);
            return new RobotMovement(intent.Robot.Id, intent.From, intent.To, CollisionResult.Success);
        }
        
        var from = intent.From;
        var to = intent.To;
        var dir = intent.Direction;
        var steps = Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y);
        
        for (int i = 1; i <= steps; i++)
        {
            var currentPos = from;
            var nextPos = from.Move(dir);
            
            if (!CanPassThrough(currentPos, nextPos, dir, map))
                return new RobotMovement(intent.Robot.Id, intent.From, from, CollisionResult.Blocked);
            
            var tile = map.GetTile(nextPos);
            var touchResult = tile.GetComponent<ITileBehaviorComponent>()?.ApplyOnTouch(intent.Robot, map);
            
            if (touchResult == TileActionResult.ReadyToFall)
            {
                intent.Robot.SetPosition(nextPos);
                return new RobotMovement(intent.Robot.Id, intent.From, nextPos, CollisionResult.Success);
            }
            
            if (resolvedPositions.ContainsValue(nextPos))
                return new RobotMovement(intent.Robot.Id, intent.From, from, CollisionResult.Blocked);
            
            var robotOnTile = robotsById.Values.FirstOrDefault(r => 
                r.Id != intent.Robot.Id && 
                r.Position == nextPos &&
                r.IsAlive &&
                !resolvedPositions.ContainsKey(r.Id));
            
            if (robotOnTile != null)
            {
                if (i < steps)
                    return new RobotMovement(intent.Robot.Id, intent.From, from, CollisionResult.Blocked);
                
                var pushTarget = nextPos.Move(dir);
                if (!CanPassThrough(nextPos, pushTarget, dir, map))
                    return new RobotMovement(intent.Robot.Id, intent.From, from, CollisionResult.Blocked);
                    
                robotOnTile.SetPosition(pushTarget);
                intent.Robot.SetPosition(nextPos);
                return new RobotMovement(intent.Robot.Id, intent.From, nextPos, CollisionResult.Pushed);
            }
            
            from = nextPos;
        }
        
        intent.Robot.SetPosition(to);
        return new RobotMovement(intent.Robot.Id, intent.From, to, CollisionResult.Success);
    }
}
