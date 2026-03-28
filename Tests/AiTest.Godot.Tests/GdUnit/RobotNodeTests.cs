namespace AiTest.Godot.Tests.GdUnit;

using AiTest.Core.Entities;
using GdUnit4;
using static GdUnit4.Assertions;
using AiTest.Core.Models;

[TestSuite]
public class RobotNodeTests
{
    [TestCase]
    [RequireGodotRuntime]
    public void Robot_ShouldCreate_WithDefaultValues()
    {
        var robot = new Robot();
        
        AssertThat(robot.Position).IsNotNull();
        AssertThat(robot.Direction).IsEqual(Direction.East);
        AssertThat(robot.Damage).IsEqual(0);
        AssertThat(robot.IsAlive).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Robot_SetDirection_UpdatesDirection()
    {
        var robot = new Robot();
        
        robot.SetDirection(Direction.East);
        
        AssertThat(robot.Direction).IsEqual(Direction.East);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Robot_Move_UpdatesPosition()
    {
        var robot = new Robot();
        var initialPos = robot.Position;
        
        robot.Move(new Position(initialPos.X + 1, initialPos.Y));
        
        AssertThat(robot.Position.X).IsEqual(initialPos.X + 1);
        AssertThat(robot.Position.Y).IsEqual(initialPos.Y);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Robot_RotateLeft_UpdatesDirection()
    {
        var robot = new Robot();
        robot.SetDirection(Direction.North);
        
        robot.RotateLeft();
        
        AssertThat(robot.Direction).IsEqual(Direction.West);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Robot_RotateRight_UpdatesDirection()
    {
        var robot = new Robot();
        robot.SetDirection(Direction.North);
        
        robot.RotateRight();
        
        AssertThat(robot.Direction).IsEqual(Direction.East);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Robot_TakeDamage_IncreasesDamage()
    {
        var robot = new Robot();
        
        robot.AddDamage(3);
        
        AssertThat(robot.Damage).IsEqual(3);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Robot_Destroyed_NotAlive()
    {
        var robot = new Robot();
        
        robot.AddDamage(10);
        
        AssertThat(robot.IsAlive).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Robot_LockedRegisters_CalculatesCorrectly()
    {
        var robot = new Robot();
        robot.AddDamage(6);
        
        AssertThat(robot.LockedRegisters).IsEqual(2);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void Robot_Reboot_ResetsDamage()
    {
        var robot = new Robot();
        robot.AddDamage(5);
        
        robot.Reboot(new Position(0, 0));
        
        AssertThat(robot.Damage).IsEqual(2);
    }
}
