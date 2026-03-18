namespace AiTest.Core.Engine;

using AiTest.Core.Entities;

public enum ActionPhase
{
    CardExecution,
    BoardElements,
    Lasers
}

public enum ActionPriority
{
    Lowest = 0,
    Low = 25,
    Medium = 50,
    High = 75,
    Highest = 100
}

public abstract class GameAction
{
    public int Priority { get; init; }
    public int PlayerId { get; init; }
    public ActionPhase Phase { get; init; }
    public abstract void Execute(IGameEngine engine);
}

public sealed class CardAction : GameAction
{
    public required Action<Entities.Robot, Models.GameMap> ExecuteCard { get; init; }
    
    public override void Execute(IGameEngine engine)
    {
        var player = engine.State.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player?.Robot.IsAlive == true && player.Robot.Status != RobotStatus.Rebooting)
        {
            ExecuteCard(player.Robot, engine.State.Map);
        }
    }
}

public sealed class BoardElementAction : GameAction
{
    public required Action<Entities.Robot, Models.GameMap> ExecuteElement { get; init; }
    
    public override void Execute(IGameEngine engine)
    {
        var player = engine.State.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player?.Robot.IsAlive == true)
        {
            ExecuteElement(player.Robot, engine.State.Map);
        }
    }
}

public sealed class LaserAction : GameAction
{
    public required Action<Entities.Robot, IGameEngine> FireLaser { get; init; }
    
    public override void Execute(IGameEngine engine)
    {
        var player = engine.State.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player?.Robot.IsAlive == true)
        {
            FireLaser(player.Robot, engine);
        }
    }
}
