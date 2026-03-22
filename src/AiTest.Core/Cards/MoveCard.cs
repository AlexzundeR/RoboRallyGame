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
            var currentTile = map.GetTile(robot.Position);
            var targetPos = robot.Position.Move(robot.Direction);
            var targetTile = map.GetTile(targetPos);
            
            if (map.IsInBounds(targetPos) && 
                targetTile.CanEnterFrom(robot.Direction) && 
                currentTile.CanExitTo(robot.Direction))
            {
                robot.Move(targetPos);
            }
            else
            {
                break;
            }
        }
    }
}
