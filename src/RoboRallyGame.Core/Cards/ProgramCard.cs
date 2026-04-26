namespace RoboRallyGame.Core.Cards;

using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Core.Engine;

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
