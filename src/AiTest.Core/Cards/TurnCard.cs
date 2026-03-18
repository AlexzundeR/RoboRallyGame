namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public sealed class TurnCard : ProgramCard
{
    public bool IsLeft { get; init; }

    public override void Execute(Robot robot, GameMap map)
    {
        if (IsLeft)
            robot.RotateLeft();
        else
            robot.RotateRight();
    }
}
