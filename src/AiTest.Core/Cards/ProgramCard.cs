namespace AiTest.Core.Cards;

using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Engine;

public enum CardType
{
    Move,
    TurnLeft,
    TurnRight,
    UTurn,
    BackUp
}

public abstract class ProgramCard
{
    public int Priority { get; init; }
    public abstract CardType Type { get; }
    public abstract string Symbol { get; }
    public abstract string Description { get; }
    public abstract MoveIntent? CreateMoveIntent(Robot robot, GameMap map, int registerIndex = -1);
}
