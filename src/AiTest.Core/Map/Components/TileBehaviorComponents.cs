namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;
using AiTest.Core.Engine;

public enum TileActionResult
{
    None,
    Moved,
    CheckpointReached,
    Repaired,
    FellIntoPit
}

public abstract class TileComponentBase : ITileComponent
{
    public Tile Tile { get; private set; }
    public Position Position => Tile.Position;

    protected TileComponentBase() { }

    public void SetTile(Tile tile) => Tile = tile;
}

public interface ITileBehaviorComponent : ITileComponent
{
    TileActionResult ApplyPostMovement(Player player, GameMap map);
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

    public TileActionResult ApplyPostMovement(Player player, GameMap map)
    {
        var robot = player.Robot;
        var strategy = player.GetConveyorStrategy();
        
        if (strategy.ShouldIgnoreConveyors(robot))
            return TileActionResult.None;
            
        var oldPos = robot.Position;
        var newPos = oldPos;
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

        if (newPos != oldPos)
        {
            robot.SetPosition(newPos);
            return TileActionResult.Moved;
        }
        
        return TileActionResult.None;
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

    public TileActionResult ApplyPostMovement(Player player, GameMap map)
    {
        if (IsClockwise)
            player.Robot.RotateRight();
        else
            player.Robot.RotateLeft();
        return TileActionResult.None;
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

    public TileActionResult ApplyPostMovement(Player player, GameMap map) => TileActionResult.None;
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

    public TileActionResult ApplyPostMovement(Player player, GameMap map)
    {
        if (Number == player.Robot.CheckpointsReached + 1)
        {
            for (int i = 0; i < Number; i++)
                player.Robot.AddCheckpoint(Position);
            return TileActionResult.CheckpointReached;
        }
        return TileActionResult.None;
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
}

    public sealed class PitBehaviorComponent : TileComponentBase, ITileBehaviorComponent
    {
        public TileActionResult ApplyPostMovement(Player player, GameMap map)
        {
            var strategy = player.GetConveyorStrategy();
            if (strategy.ShouldIgnorePits(player.Robot))
                return TileActionResult.None;

            player.Robot.LoseLifeToken();
            return TileActionResult.FellIntoPit;
        }

        public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
        public void ApplyOnEnter(Player player, GameState state) { }
    }

    public sealed class RepairSiteBehaviorComponent : TileComponentBase, ITileBehaviorComponent, IEndOfRoundComponent
    {
        public int RepairAmount { get; }

        public RepairSiteBehaviorComponent(int repairAmount = 1)
        {
            RepairAmount = repairAmount;
        }

        public TileActionResult ApplyPostMovement(Player player, GameMap map) => TileActionResult.None;

        public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
        public void ApplyOnEnter(Player player, GameState state) { }

        public TileActionResult ApplyEndOfRound(Player player)
        {
            if (player.Robot.Damage > 0)
            {
                player.Robot.Repair(RepairAmount);
                return TileActionResult.Repaired;
            }
            return TileActionResult.None;
        }
    }
