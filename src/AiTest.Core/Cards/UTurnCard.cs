namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public sealed class UTurnCard : ProgramCard
{
    public override void Execute(Robot robot, GameMap map)
    {
        robot.Rotate180();
    }
}
