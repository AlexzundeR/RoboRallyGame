namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public sealed class MoveCard : ProgramCard
{
    public int Spaces { get; init; }

    public override void Execute(Robot robot, GameMap map)
    {
        for (int i = 0; i < Spaces; i++)
        {
            var newPos = robot.Position.Move(robot.Direction);
            if (map.IsInBounds(newPos) && map.GetTile(newPos).Type != TileType.Wall)
            {
                robot.Move(newPos);
            }
            else
            {
                break;
            }
        }
    }
}
