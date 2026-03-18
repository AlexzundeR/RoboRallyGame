namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public abstract class ProgramCard
{
    public int Priority { get; init; }
    public abstract void Execute(Robot robot, GameMap map);
}
