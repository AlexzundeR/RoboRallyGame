// namespace RoboRallyGame.Core.Engine;

// using RoboRallyGame.Core.Entities;

// public enum ActionPhase
// {
//     CardExecution,
//     BoardElements,
//     Lasers
// }

// public enum ActionPriority
// {
//     Lowest = 0,
//     Low = 25,
//     Medium = 50,
//     High = 75,
//     Highest = 100
// }

// public abstract class GameAction
// {
//     public int Priority { get; init; }
//     public int PlayerId { get; init; }
//     public ActionPhase Phase { get; init; }
//     public abstract void Execute(IGameEngine engine);
// }

// public sealed class CardAction : GameAction
// {
//     public required Action<Robot, Models.GameMap> ExecuteCard { get; init; }
    
//     public override void Execute(IGameEngine engine)
//     {
//         var player = engine.State.Players.First(p => p.Id == PlayerId);
//         if (player.Robot.IsAlive == true && !player.Robot.IsRebooting)
//         {
//             ExecuteCard(player.Robot, engine.State.Map);
//         }
//     }
// }

// public sealed class BoardElementAction : GameAction
// {
//     public required Action<Robot, Models.GameMap> ExecuteElement { get; init; }
    
//     public override void Execute(IGameEngine engine)
//     {
//         var player = engine.State.Players.FirstOrDefault(p => p.Id == PlayerId);
//         if (player?.Robot.IsAlive == true)
//         {
//             ExecuteElement(player.Robot, engine.State.Map);
//         }
//     }
// }

// public sealed class LaserAction : GameAction
// {
//     public required Action<Robot, IGameEngine> FireLaser { get; init; }
    
//     public override void Execute(IGameEngine engine)
//     {
//         var player = engine.State.Players.FirstOrDefault(p => p.Id == PlayerId);
//         if (player?.Robot.IsAlive == true && !player.Robot.IsRebooting)
//         {
//             FireLaser(player.Robot, engine);
//         }
//     }
// }
