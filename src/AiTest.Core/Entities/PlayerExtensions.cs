namespace AiTest.Core.Entities;

using System.Collections.Generic;
using AiTest.Core.Upgrades;

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
}
