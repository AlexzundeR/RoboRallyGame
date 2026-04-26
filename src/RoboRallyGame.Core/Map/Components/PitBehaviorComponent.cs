namespace RoboRallyGame.Core.Models.Components;

using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Engine;

public sealed class PitBehaviorComponent : TileComponentBase, ITileBehaviorComponent
{
    public TileEffect ApplyEffect(Player player, GameMap map)
    {
        if (map.GetTile(player.Robot.Position).Type == TileType.Pit)
        {
            player.Robot.LoseLifeToken();
            return new TileEffect(null, TileActionResult.FellIntoPit);
        }
        return new TileEffect(null, TileActionResult.None);
    }

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }

    public TileActionResult ApplyOnTouch(Robot robot, GameMap map)
    {
        return TileActionResult.ReadyToFall;
    }
}
