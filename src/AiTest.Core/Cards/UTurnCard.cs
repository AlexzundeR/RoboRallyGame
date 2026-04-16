namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Engine;

public sealed class UTurnCard : ProgramCard
{
    public override CardType Type => CardType.UTurn;
    public override string Symbol => "⤵";
    public override string Description => "Turn around 180°";

    public override MoveIntent? CreateMoveIntent(Robot robot, GameMap map, int registerIndex)
    {
        return new MoveIntent
        {
            Robot = robot,
            From = robot.Position,
            To = robot.Position,
            Direction = robot.Direction.TurnAround(),
            Priority = Priority,
            RegisterIndex = registerIndex,
            Type = MoveType.Rotation
        };
    }
}
