namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;
using AiTest.Core.Engine;

public sealed class CheckpointBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public int Number { get; }

    public CheckpointBehaviorComponent(int number)
    {
        Number = number;
    }

    public TileEffect ApplyEffect(Player player, GameMap map)
    {
        if (player.Robot.HasCheckpoint(Number))
            return new TileEffect(null, TileActionResult.None);

        if (Number == player.Robot.CheckpointsReached + 1)
        {
            player.Robot.AddCheckpoint(Number, Position);
            return new TileEffect(null, TileActionResult.CheckpointReached);
        }
        return new TileEffect(null, TileActionResult.None);
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;
}
