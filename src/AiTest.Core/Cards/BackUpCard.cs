namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public sealed class BackUpCard : ProgramCard
{
    public override void Execute(Robot robot, GameMap map)
    {
        var oppositeDir = robot.Direction.TurnAround();
        var currentTile = map.GetTile(robot.Position);
        var newPos = robot.Position.Move(oppositeDir);
        var targetTile = map.GetTile(newPos);
        
        if (map.IsInBounds(newPos) && 
            targetTile.CanEnterFrom(oppositeDir) && 
            currentTile.CanExitTo(oppositeDir))
        {
            robot.Move(newPos);
        }
    }
}
