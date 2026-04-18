namespace AiTest.Core.Entities;

using AiTest.Core.Models;

public interface IRobotLifecycleComponent
{
    void OnTakeDamage(Robot robot, int damage);
    void OnLoseLifeToken(Robot robot);
    void OnReboot(Robot robot, Position spawnPos);
    void OnPowerDown(Robot robot);
    void OnWakeUp(Robot robot);
    void OnRepair(Robot robot, int amount);
}

public sealed class DefaultRobotLifecycle : IRobotLifecycleComponent
{
    public static readonly DefaultRobotLifecycle Instance = new();
    
    public void OnTakeDamage(Robot robot, int damage)
    {
        if (damage < 0)
        {
            robot.SetDamage(Math.Max(0, robot.Damage + damage));
            return;
        }
        robot.SetDamage(robot.Damage + damage);
    }

    public void OnLoseLifeToken(Robot robot)
    {
        robot.SetLifeTokens(robot.LifeTokens - 1);
        robot.SetStatus(RobotStatus.Rebooting);
        robot.SetDamage(0);
    }

    public void OnReboot(Robot robot, Position spawnPos)
    {
        robot.SetPosition(spawnPos);
        robot.SetDirection(Direction.North);
        robot.SetStatus(RobotStatus.Active);
        robot.SetDamage(2);
        robot.SetCheckpoints(0);
    }

    public void OnPowerDown(Robot robot)
    {
        robot.SetStatus(RobotStatus.PowerDown);
    }

    public void OnWakeUp(Robot robot)
    {
        robot.SetStatus(RobotStatus.Active);
        robot.ClearLastCheckpoint();
    }

    public void OnRepair(Robot robot, int amount)
    {
        robot.SetDamage(Math.Max(0, robot.Damage - amount));
    }
}