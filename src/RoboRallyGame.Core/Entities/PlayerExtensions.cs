namespace RoboRallyGame.Core.Entities;

using System.Collections.Generic;
using RoboRallyGame.Core.Upgrades;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Core.Engine;

public static class PlayerExtensions
{
    public static IConveyorStrategy GetConveyorStrategy(this Player player)
    {
        var strategies = new List<IConveyorStrategy> { DefaultConveyorStrategy.Instance };
        
        foreach (var upgrade in player.AllUpgrades)
        {
            var strategy = upgrade.GetConveyorStrategy();
            if (strategy != null)
                strategies.Add(strategy);
        }
        
        return new CompositeConveyorStrategy(strategies);
    }

    public static ILaserStrategy GetLaserStrategy(this Player player)
    {
        var strategies = new List<ILaserStrategy> { DefaultLaserStrategy.Instance };
        
        foreach (var upgrade in player.AllUpgrades)
        {
            var strategy = upgrade.GetLaserStrategy();
            if (strategy != null)
                strategies.Add(strategy);
        }
        
        return new CompositeLaserStrategy(strategies);
    }

    public static IMovementStrategy GetMovementStrategy(this Player player)
    {
        var strategies = new List<IMovementStrategy> { DefaultMovementStrategy.Instance };
        
        foreach (var upgrade in player.AllUpgrades)
        {
            var strategy = upgrade.GetMovementStrategy();
            if (strategy != null)
                strategies.Add(strategy);
        }
        
        return new CompositeMovementStrategy(strategies);
    }

    public static IDamageStrategy GetDamageStrategy(this Player player)
    {
        var strategies = new List<IDamageStrategy> { DefaultDamageStrategy.Instance };
        
        foreach (var upgrade in player.AllUpgrades)
        {
            var strategy = upgrade.GetDamageStrategy();
            if (strategy != null)
                strategies.Add(strategy);
        }
        
        return new CompositeDamageStrategy(strategies);
    }
}
