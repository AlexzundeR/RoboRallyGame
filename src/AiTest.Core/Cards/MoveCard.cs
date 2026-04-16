namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Engine;

public sealed class MoveCard : ProgramCard
{
    public int Spaces { get; init; }
    public override CardType Type => CardType.Move;
    public override string Symbol => $"M{Spaces}";
    public override string Description => Spaces == 1 ? "Move 1 space" : $"Move {Spaces} spaces";

    public override MoveIntent? CreateMoveIntent(Robot robot, GameMap map, int registerIndex)
    {
        var pos = robot.Position;
        var moveDir = robot.Direction;
        var entrySide = moveDir.TurnAround();
        
        for (int i = 0; i < Spaces; i++)
        {
            var currentTile = map.GetTile(pos);
            var next = pos.Move(moveDir);
            if (!map.IsInBounds(next))
                break;
            var nextTile = map.GetTile(next);
            if (!nextTile.CanEnterFrom(entrySide))
                break;
            if (!currentTile.CanExitTo(moveDir))
                break;
            pos = next;
        }
        
        if (pos == robot.Position)
            return null;
            
        return new MoveIntent
        {
            Robot = robot,
            From = robot.Position,
            To = pos,
            Direction = moveDir,
            Priority = Priority,
            RegisterIndex = registerIndex,
            Type = MoveType.Normal
        };
    }
}
