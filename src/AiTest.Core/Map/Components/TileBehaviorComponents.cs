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
    void ApplyPostMovement(Player player, GameMap map);
    void ApplyOnLaserPhase(LaserShot shot, GameState state);
    void ApplyOnEnter(Player player, GameState state);
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

    public void ApplyPostMovement(Player player, GameMap map)
    {
        var robot = player.Robot;
        var strategy = player.GetConveyorStrategy();
        
        if (strategy.ShouldIgnoreConveyors(robot))
            return;
            
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
    public void ApplyOnEnter(Player player, GameState state) { }
}

public sealed class GearBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public bool IsClockwise { get; }

    public GearBehaviorComponent(bool clockwise)
    {
        IsClockwise = clockwise;
    }

    public void ApplyPostMovement(Player player, GameMap map)
    {
        if (IsClockwise)
            player.Robot.RotateRight();
        else
            player.Robot.RotateLeft();
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
}

public sealed class LaserBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public Direction Direction { get; }

    public LaserBehaviorComponent(Direction direction)
    {
        Direction = direction;
    }

    public void ApplyPostMovement(Player player, GameMap map) { }
    public void ApplyOnEnter(Player player, GameState state) { }

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

    public void ApplyPostMovement(Player player, GameMap map) { }
    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }

    public void ApplyOnEnter(Player player, GameState state)
    {
        if (Number == player.Robot.CheckpointsReached + 1)
        {
            player.Robot.AddCheckpoint(Position);
        }
    }
}
