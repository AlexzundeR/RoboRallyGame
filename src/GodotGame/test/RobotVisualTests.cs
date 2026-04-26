namespace RoboRallyGame.Tests.GdUnit.Robot;

using GdUnit4;
using static GdUnit4.Assertions;
using Godot;

using RobotNode = Godot.RobotNode;

[TestSuite]
public class RobotVisualTests
{
    [TestCase]
    [RequireGodotRuntime]
    public void GodotRobot_GetDirection_ShouldReturnDefaultNorth()
    {
        var robot = new RobotNode();
        
        AssertThat(robot.GetDirection()).IsEqual(0);
        
        robot.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void GodotRobot_SetDirection_ShouldUpdateInternalDirection()
    {
        var robot = new RobotNode();
        
        robot.SetDirection(0);
        AssertThat(robot.GetDirection()).IsEqual(0);
        
        robot.SetDirection(1);
        AssertThat(robot.GetDirection()).IsEqual(1);
        
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
