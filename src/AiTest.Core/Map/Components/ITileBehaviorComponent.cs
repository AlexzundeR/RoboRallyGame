namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;
using AiTest.Core.Engine;

public interface ITileBehaviorComponent : ITileComponent
{
    TileEffect ApplyEffect(Player player, GameMap map);
    void ApplyOnLaserPhase(LaserShot shot, GameState state);
    void ApplyOnEnter(Player player, GameState state);
    TileActionResult ApplyOnTouch(Robot robot, GameMap map);
}
