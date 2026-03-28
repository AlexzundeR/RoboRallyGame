namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;
using AiTest.Core.Engine;

public abstract class TileComponentBase : ITileComponent
{
    public Tile Tile { get; private set; }
    public Position Position => Tile.Position;

    protected TileComponentBase() { }

    public void SetTile(Tile tile) => Tile = tile;
}

public interface ITileBehaviorComponent : ITileComponent
{
    void ApplyPostMovement(Robot robot, GameMap map);
    void ApplyOnLaserPhase(LaserShot shot, GameState state);
    void ApplyOnEnter(Robot robot, GameState state);
}

public sealed class ConveyorBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public ConveyorDirection Direction { get; }
    public int Steps { get; }
    public bool IsExpress => Steps == 2;

    public ConveyorBehaviorComponent(ConveyorDirection direction, bool isExpress)
    {
        Direction = direction;
        Steps = isExpress ? 2 : 1;
    }

    public void ApplyPostMovement(Robot robot, GameMap map)
    {
        var newPos = robot.Position;
        var dir = (Direction)Direction;

        for (int i = 0; i < Steps; i++)
        {
            var next = newPos.Move(dir);
            
            if (!map.IsInBounds(next))
                break;
                
            var currentTile = map.GetTile(newPos);
            var targetTile = map.GetTile(next);
            
            if (!targetTile.CanEnterFrom(dir) || !currentTile.CanExitTo(dir))
                break;
                
            newPos = next;
        }

        robot.SetPosition(newPos);
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Robot robot, GameState state) { }
}

public sealed class GearBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public bool IsClockwise { get; }

    public GearBehaviorComponent(bool clockwise)
    {
        IsClockwise = clockwise;
    }

    public void ApplyPostMovement(Robot robot, GameMap map)
    {
        if (IsClockwise)
            robot.RotateRight();
        else
            robot.RotateLeft();
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Robot robot, GameState state) { }
}

public sealed class LaserBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public Direction Direction { get; }

    public LaserBehaviorComponent(Direction direction)
    {
        Direction = direction;
    }

    public void ApplyPostMovement(Robot robot, GameMap map) { }
    public void ApplyOnEnter(Robot robot, GameState state) { }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state)
    {
    }
}

public sealed class CheckpointBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public int Number { get; }

    public CheckpointBehaviorComponent(int number)
    {
        Number = number;
    }

    public void ApplyPostMovement(Robot robot, GameMap map) { }
    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }

    public void ApplyOnEnter(Robot robot, GameState state)
    {
        if (Number == robot.CheckpointsReached + 1)
        {
            robot.AddCheckpoint(Position);
        }
    }
}
