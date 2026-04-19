namespace AiTest.Core.Entities;

using AiTest.Core.Models;

public enum RoundPreparationResult
{
    None,
    WokeUp,
    Rebooted,
    Destroyed
}

public interface IRobotLifecycleComponent
{
    void OnTakeDamage(Robot robot, int damage);
    void OnAfterDamage(Robot robot, int damage);
    void OnLoseLifeToken(Robot robot);
    void OnReboot(Robot robot, Position spawnPos);  
    RoundPreparationResult PrepareForRound(Robot robot, Position defaultSpawn);
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

    public void OnAfterDamage(Robot robot, int damage)
    {
        if (robot.Damage >= 10)
        {
            if (robot.LifeTokens > 0)
                robot.LoseLifeToken();
            else
                robot.SetStatus(RobotStatus.Destroyed);
        }
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

    public RoundPreparationResult PrepareForRound(Robot robot, Position defaultSpawn)
    {
        if (robot.Status == RobotStatus.PowerDown)
        {
            robot.WakeUp();
            robot.SetDamage(0);
            return RoundPreparationResult.WokeUp;
        }
        
        if (robot.Status == RobotStatus.Rebooting)
        {
            if (robot.LifeTokens <= 0)
            {
                robot.SetStatus(RobotStatus.Destroyed);
                return RoundPreparationResult.Destroyed;
            }
            
            var spawnPos = robot.LastCheckpointPosition ?? defaultSpawn;
            robot.Reboot(spawnPos);
            return RoundPreparationResult.Rebooted;
        }
        
        return RoundPreparationResult.None;
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