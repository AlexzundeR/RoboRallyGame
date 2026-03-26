namespace AiTest.Core.Models.Components;

using AiTest.Core.Entities;

public interface ITileBehaviorComponent : ITileComponent
{
    void ApplyPostMovement(Robot robot);
}

public sealed class ConveyorBehaviorComponent : ITileBehaviorComponent
{
    public ConveyorDirection Direction { get; }
    public int Steps { get; }
    public bool IsExpress => Steps == 2;

    public ConveyorBehaviorComponent(ConveyorDirection direction, bool isExpress)
    {
        Direction = direction;
        Steps = isExpress ? 2 : 1;
    }

    public void ApplyPostMovement(Robot robot)
    {
        var newPos = robot.Position;

        for (int i = 0; i < Steps; i++)
        {
            var next = Direction switch
            {
                ConveyorDirection.North => new Position(newPos.X, newPos.Y - 1),
                ConveyorDirection.South => new Position(newPos.X, newPos.Y + 1),
                ConveyorDirection.East => new Position(newPos.X + 1, newPos.Y),
                ConveyorDirection.West => new Position(newPos.X - 1, newPos.Y),
                _ => newPos
            };

            if (newPos != next)
                newPos = next;
        }

        robot.SetPosition(newPos);
    }
}

public sealed class GearBehaviorComponent : ITileBehaviorComponent
{
    public bool IsClockwise { get; }

    public GearBehaviorComponent(bool clockwise)
    {
        IsClockwise = clockwise;
    }

    public void ApplyPostMovement(Robot robot)
    {
        if (IsClockwise)
            robot.RotateRight();
        else
            robot.RotateLeft();
    }
}
