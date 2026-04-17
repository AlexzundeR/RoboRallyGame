namespace AiTest.Tests.GdUnit.Robot;

using AiTest.Core.Models;
using GdUnit4;
using static GdUnit4.Assertions;
using Godot;

using RobotNode = AiTest.Godot.RobotNode;

[TestSuite]
public class RobotVisualTests
{
    [TestCase]
    [RequireGodotRuntime]
    public void GodotRobot_GetDirection_ShouldReturnDefaultNorth()
    {
        var robot = new RobotNode();
        
        AssertThat(robot.GetDirection()).IsEqual(Direction.North);
        
        robot.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GodotRobot_SetDirection_ShouldUpdateInternalDirection()
    {
        var robot = new RobotNode();
        
        robot.SetDirection(Direction.North);
        AssertThat(robot.GetDirection()).IsEqual(Direction.North);
        
        robot.SetDirection(Direction.East);
        AssertThat(robot.GetDirection()).IsEqual(Direction.East);
        
        robot.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GodotRobot_IsAlive_ShouldReturnTrue()
    {
        var robot = new RobotNode();
        
        AssertThat(robot.IsAlive()).IsTrue();
        
        robot.QueueFree();
    }
}
