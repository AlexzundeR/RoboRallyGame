namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;
using AiTest.Core.Engine;

public sealed class GearBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public bool IsClockwise { get; }

    public GearBehaviorComponent(bool clockwise)
    {
        IsClockwise = clockwise;
    }

    public TileEffect ApplyEffect(Player player, GameMap map)
    {
        if (IsClockwise)
            player.Robot.RotateRight();
        else
            player.Robot.RotateLeft();
        return new TileEffect(null, TileActionResult.None);
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;
}
