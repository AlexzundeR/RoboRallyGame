namespace AiTest.Tests.GdUnit.Animations;

using AiTest.Godot.Animations;
using AiTest.Core.Models;
using GdUnit4;
using static GdUnit4.Assertions;
using Godot;

using RobotNode = Godot.RobotNode;

[TestSuite]
public class MoveAnimationTests
{
    [TestCase]
    [RequireGodotRuntime]
    public void MoveAnimation_ShouldCreate_WithRobotAndPositions()
    {
        var robot = new RobotNode();
        
        var animation = new MoveAnimation
        {
            Robot = robot,
            From = new Position(0, 0),
            To = new Position(3, 3),
            Duration = 0.1f
        };
        
        AssertThat(animation.Robot).IsNotNull();
        AssertThat(animation.From).IsEqual(new Position(0, 0));
        AssertThat(animation.To).IsEqual(new Position(3, 3));
        AssertThat(animation.Duration).IsEqual(0.1f);
        
        robot.QueueFree();
    }
}
