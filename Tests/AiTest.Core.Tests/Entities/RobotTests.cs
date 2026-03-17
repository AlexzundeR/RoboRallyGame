namespace AiTest.Core.Tests.Entities;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public class RobotTests
{
    [Fact]
    public void NewRobot_ShouldHaveDefaultValues()
    {
        var robot = new Robot { Id = 1, Name = "TestBot" };
        
        Assert.Equal(1, robot.Id);
        Assert.Equal("TestBot", robot.Name);
        Assert.Equal(Direction.North, robot.Direction);
        Assert.Equal(0, robot.Damage);
        Assert.Equal(3, robot.Energy);
        Assert.Equal(RobotStatus.Active, robot.Status);
        Assert.True(robot.IsAlive);
    }

    [Fact]
    public void Move_ShouldUpdatePosition()
    {
        var robot = new Robot { Position = new Position(5, 5) };
        
        robot.Move(new Position(6, 5));
        
        Assert.Equal(6, robot.Position.X);
        Assert.Equal(5, robot.Position.Y);
    }

    [Fact]
    public void RotateLeft_ShouldTurn90DegreesLeft()
    {
        var robot = new Robot { Direction = Direction.North };
        
        robot.RotateLeft();
        
        Assert.Equal(Direction.West, robot.Direction);
    }

    [Fact]
    public void RotateRight_ShouldTurn90DegreesRight()
    {
        var robot = new Robot { Direction = Direction.North };
        
        robot.RotateRight();
        
        Assert.Equal(Direction.East, robot.Direction);
    }

    [Fact]
    public void Rotate180_ShouldTurnAround()
    {
        var robot = new Robot { Direction = Direction.North };
        
        robot.Rotate180();
        
        Assert.Equal(Direction.South, robot.Direction);
    }

    [Fact]
    public void TakeDamage_LessThan10_ShouldIncreaseDamage()
    {
        var robot = new Robot { Damage = 5 };
        
        robot.TakeDamage(3);
        
        Assert.Equal(8, robot.Damage);
        Assert.Equal(RobotStatus.Active, robot.Status);
    }

    [Fact]
    public void TakeDamage_10OrMore_ShouldTriggerReboot()
    {
        var robot = new Robot { Damage = 8 };
        
        robot.TakeDamage(2);
        
        Assert.Equal(0, robot.Damage);
        Assert.Equal(RobotStatus.Rebooting, robot.Status);
    }

    [Fact]
    public void Reboot_ShouldResetState()
    {
        var robot = new Robot 
        { 
            Damage = 5, 
            Status = RobotStatus.Rebooting,
            CheckpointsReached = 2 
        };
        
        robot.Reboot(new Position(1, 1));
        
        Assert.Equal(new Position(1, 1), robot.Position);
        Assert.Equal(Direction.North, robot.Direction);
        Assert.Equal(RobotStatus.Active, robot.Status);
        Assert.Equal(2, robot.Damage);
        Assert.Equal(0, robot.CheckpointsReached);
    }

    [Fact]
    public void PowerDown_ShouldSetStatusToPowerDown()
    {
        var robot = new Robot();
        
        robot.PowerDown();
        
        Assert.Equal(RobotStatus.PowerDown, robot.Status);
    }

    [Fact]
    public void WakeUp_ShouldSetStatusToActive()
    {
        var robot = new Robot { Status = RobotStatus.PowerDown };
        
        robot.WakeUp();
        
        Assert.Equal(RobotStatus.Active, robot.Status);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 1)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    [InlineData(8, 4)]
    [InlineData(9, 5)]
    public void LockedRegisters_ShouldReturnCorrectCount(int damage, int expected)
    {
        var robot = new Robot { Damage = damage };
        
        Assert.Equal(expected, robot.LockedRegisters);
    }

    [Fact]
    public void IsPowerDown_WhenPoweredDown_ShouldReturnTrue()
    {
        var robot = new Robot { Status = RobotStatus.PowerDown };
        
        Assert.True(robot.IsPowerDown);
    }

    [Fact]
    public void NeedsReboot_WhenRebooting_ShouldReturnTrue()
    {
        var robot = new Robot { Status = RobotStatus.Rebooting };
        
        Assert.True(robot.NeedsReboot);
    }

    [Fact]
    public void Reboot_ShouldAddTwoDamage()
    {
        var robot = new Robot { Damage = 0 };
        
        robot.Reboot(new Position(0, 0));
        
        Assert.Equal(2, robot.Damage);
    }

    [Fact]
    public void Reboot_ShouldResetCheckpoints()
    {
        var robot = new Robot { CheckpointsReached = 3 };
        
        robot.Reboot(new Position(0, 0));
        
        Assert.Equal(0, robot.CheckpointsReached);
    }

    [Fact]
    public void LifeTokens_DefaultValue_ShouldBeThree()
    {
        var robot = new Robot();
        
        Assert.Equal(3, robot.LifeTokens);
    }

    [Fact]
    public void TakeDamage_WhenDestroyed_ShouldLoseLifeToken()
    {
        var robot = new Robot { LifeTokens = 3, Damage = 9 };
        
        robot.TakeDamage(1);
        
        Assert.Equal(2, robot.LifeTokens);
        Assert.Equal(RobotStatus.Rebooting, robot.Status);
    }

    [Fact]
    public void TakeDamage_WhenNoLifeTokens_ShouldBeDestroyed()
    {
        var robot = new Robot { LifeTokens = 0, Damage = 9 };
        
        robot.TakeDamage(1);
        
        Assert.Equal(RobotStatus.Destroyed, robot.Status);
    }
}
