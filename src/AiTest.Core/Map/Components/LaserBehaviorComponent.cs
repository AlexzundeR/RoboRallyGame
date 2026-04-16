namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;
using AiTest.Core.Engine;

public sealed class LaserBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public Direction Direction { get; }

    public LaserBehaviorComponent(Direction direction)
    {
        Direction = direction;
    }

    public TileEffect ApplyEffect(Player player, GameMap map) => new(null, TileActionResult.None);

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;
}
