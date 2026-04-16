namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Engine;

public sealed class BackUpCard : ProgramCard
{
    public override CardType Type => CardType.BackUp;
    public override string Symbol => "⬇";
    public override string Description => "Move backward 1 space";

    public override MoveIntent? CreateMoveIntent(Robot robot, GameMap map, int registerIndex)
    {
        var moveDir = robot.Direction.TurnAround();
        var entrySide = robot.Direction;
        var targetPos = robot.Position.Move(moveDir);
        
        if (!map.IsInBounds(targetPos))
            return null;
            
        var currentTile = map.GetTile(robot.Position);
        var targetTile = map.GetTile(targetPos);
        
        if (!targetTile.CanEnterFrom(entrySide))
            return null;
        if (!currentTile.CanExitTo(moveDir))
            return null;
            
        return new MoveIntent
        {
            Robot = robot,
            From = robot.Position,
            To = targetPos,
            Direction = moveDir,
            Priority = Priority,
            RegisterIndex = registerIndex,
            Type = MoveType.Normal
        };
    }
}
