namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public sealed class BackUpCard : ProgramCard
{
    public override void Execute(Robot robot, GameMap map)
    {
        var newPos = robot.Position.Move(robot.Direction.TurnAround());
        if (map.IsInBounds(newPos) && map.GetTile(newPos).Type != TileType.Wall)
        {
            robot.Move(newPos);
        }
    }
}
