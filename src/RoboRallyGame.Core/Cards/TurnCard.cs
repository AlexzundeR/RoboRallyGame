namespace RoboRallyGame.Core.Cards;

using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Core.Engine;

public sealed class TurnCard : ProgramCard
{
    public bool IsLeft { get; init; }
    public override CardType Type => IsLeft ? CardType.TurnLeft : CardType.TurnRight;
    public override string Symbol => IsLeft ? "↺" : "↻";
    public override string Description => IsLeft ? "Turn left 90°" : "Turn right 90°";

    public override MoveIntent? CreateMoveIntent(Robot robot, GameMap map, int registerIndex)
    {
        var fromDir = robot.Direction;
        var toDir = IsLeft ? fromDir.TurnLeft() : fromDir.TurnRight();
        
        return new MoveIntent
        {
            Robot = robot,
            From = robot.Position,
            To = robot.Position,
            Direction = toDir,
            Priority = Priority,
            RegisterIndex = registerIndex,
            Type = MoveType.Rotation
        };
    }
}
