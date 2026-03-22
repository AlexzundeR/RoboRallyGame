namespace AiTest.Godot.Tests.Robot;

using Godot;
using AiTest.Core.Models;
using AiTest.Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class RobotVisualTests
{
    [TestCase]
    [RequireGodotRuntime]
    public void GodotRobot_SetPosition_ShouldUpdatePosition()
    {
        var robot = new Robot();
        
        robot.SetPosition(new Vector2(100, 200));
        
        AssertThat(robot.Position.X).IsEqual(100);
        AssertThat(robot.Position.Y).IsEqual(200);
        
        robot.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GodotRobot_SetDirection_ShouldUpdateRotation()
    {
        var robot = new Robot();
        
        robot.SetDirection(Direction.North);
        AssertThat(robot.RotationDegrees).IsEqual(0);
        
        robot.SetDirection(Direction.East);
        AssertThat(robot.RotationDegrees).IsEqual(90);
        
        robot.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GodotRobot_DefaultPosition_ShouldBeZero()
    {
        var robot = new Robot();
        
        AssertThat(robot.Position).IsEqual(Vector2.Zero);
        
        robot.QueueFree();
    }
}