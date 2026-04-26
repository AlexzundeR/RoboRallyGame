namespace RoboRallyGame.Core.Models.Components;

using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Engine;
using RoboRallyGame.Core.Upgrades;

public sealed class RepairSiteBehaviorComponent : TileComponentBase, ITileBehaviorComponent, IEndOfRoundComponent
{
    public int RepairAmount { get; }
    public bool GrantsUpgrade { get; }

    public RepairSiteBehaviorComponent(int repairAmount = 1, bool grantsUpgrade = false)
    {
        RepairAmount = repairAmount;
        GrantsUpgrade = grantsUpgrade;
    }

    public TileEffect ApplyEffect(Player player, GameMap map) => new(null, TileActionResult.None);

    public void ApplyOnLaserPhase(LaserShot shot, GameState state) { }
    public void ApplyOnEnter(Player player, GameState state) { }
    public TileActionResult ApplyOnTouch(Robot robot, GameMap map) => TileActionResult.None;

    public TileActionResult ApplyEndOfRound(Player player)
    {
        var result = TileActionResult.None;

        if (player.Robot.Damage > 0)
        {
            player.Robot.Repair(RepairAmount);
            result = TileActionResult.Repaired;
        }

        if (GrantsUpgrade)
        {
            TryGrantUpgrade(player);
        }

        return result;
    }

    private void TryGrantUpgrade(Player player)
    {
        var upgradeEffect = UpgradeCatalog.AllUpgrades
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (upgradeEffect != null)
        {
            var effect = UpgradeCatalog.CreateEffect(upgradeEffect);
            player.AddUpgrade(effect);
        }
    }
}
