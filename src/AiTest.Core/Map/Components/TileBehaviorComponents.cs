namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;
using AiTest.Core.Engine;
using AiTest.Core.Upgrades;

public enum TileActionResult
{
    None,
    Moved,
    CheckpointReached,
    Repaired,
    FellIntoPit,
    ReadyToFall
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
    TileActionResult ApplyOnTouch(Robot robot, GameMap map);
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

    public TileActionResult ApplyPostMovement(Player player, GameMap map) => TileActionResult.None;

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;

    public MoveIntent? TryCreateIntent(Robot robot, GameMap map)
    {
        var dir = (Direction)Direction;
        var entrySide = dir.TurnAround();
        var newPos = robot.Position;

        for (int i = 0; i < Steps; i++)
        {
            var next = newPos.Move(dir);
            
            if (!map.IsInBounds(next))
                break;
                
            var currentTile = map.GetTile(newPos);
            var targetTile = map.GetTile(next);
            
            if (!targetTile.CanEnterFrom(entrySide) || !currentTile.CanExitTo(dir))
                break;
                
            newPos = next;
        }

        if (newPos == robot.Position)
            return null;

        return new MoveIntent
        {
            Robot = robot,
            From = robot.Position,
            To = newPos,
            Direction = dir,
            Priority = 0,
            RegisterIndex = -1,
            Type = MoveType.Conveyor
        };
    }
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
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;
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

    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;
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
        if (player.Robot.HasCheckpoint(Number))
            return TileActionResult.None;
            
        if (Number == player.Robot.CheckpointsReached + 1)
        {
            player.Robot.AddCheckpoint(Number, Position);
            return TileActionResult.CheckpointReached;
        }
        return TileActionResult.None;
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;
}

public sealed class PitBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public TileActionResult ApplyPostMovement(Player player, GameMap map)
    {
        if (map.GetTile(player.Robot.Position).Type == TileType.Pit)
        {
            player.Robot.LoseLifeToken();
            return TileActionResult.FellIntoPit;
        }
        return TileActionResult.None;
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }

    public TileActionResult ApplyOnTouch(Robot robot, GameMap map)
    {
        return TileActionResult.ReadyToFall;
    }
}

public sealed class RepairSiteBehaviorComponent : TileComponentBase, ITileBehaviorComponent, IEndOfRoundComponent
{
    public int RepairAmount { get; }
    public bool GrantsUpgrade { get; }

    public RepairSiteBehaviorComponent(int repairAmount = 1, bool grantsUpgrade = false)
    {
        RepairAmount = repairAmount;
        GrantsUpgrade = grantsUpgrade;
    }

    public TileActionResult ApplyPostMovement(Player player, GameMap map) => TileActionResult.None;

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;

    public TileActionResult ApplyEndOfRound(Player player)
    {
        var result = TileActionResult.None;

        if (player.Robot.Damage > 0)
        {
            player.Robot.Repair(RepairAmount);
            result = TileActionResult.Repaired;
        }

        if (GrantsUpgrade)
        {
            TryGrantUpgrade(player);
        }

        return result;
    }

    private void TryGrantUpgrade(Player player)
    {
        var upgradeEffect = UpgradeCatalog.AllUpgrades
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (upgradeEffect != null)
        {
            var effect = UpgradeCatalog.CreateEffect(upgradeEffect);
            player.AddUpgrade(effect);
        }
    }
}
