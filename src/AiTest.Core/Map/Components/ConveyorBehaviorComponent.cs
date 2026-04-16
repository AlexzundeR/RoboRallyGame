namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;
using AiTest.Core.Engine;
using AiTest.Core.Upgrades;

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

    public TileEffect ApplyEffect(Player player, GameMap map)
    {
        var conveyorStrategy = player.GetConveyorStrategy();
        if (conveyorStrategy.ShouldIgnoreConveyors(player.Robot))
            return new TileEffect(null, TileActionResult.None);

        return new TileEffect(TryCreateIntent(player.Robot, map), TileActionResult.None);
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;

    private MoveIntent? TryCreateIntent(Robot robot, GameMap map)
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
