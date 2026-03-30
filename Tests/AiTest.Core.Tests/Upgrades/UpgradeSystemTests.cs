namespace AiTest.Core.Tests.Upgrades;

using AiTest.Core.Cards;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Upgrades;
using AiTest.Core.Upgrades.Permanent;
using AiTest.Core.Upgrades.Temporary;

public class UpgradeSystemTests
{
    private static void ExecuteRegister(GameEngine engine, int registerIndex)
    {
        engine.BeginRegisterExecution(registerIndex);
        while (engine.TryExecuteNextStep() == StepResult.Executed) { }
    }

    private static Player CreateWithHand(params ProgramCard[] cards)
    {
        var hand = new ProgramCard?[9];
        for (int i = 0; i < cards.Length && i < 9; i++)
            hand[i] = cards[i];
        return new Player(null, hand);
    }

    [Fact]
    public void UpgradeCatalog_ShouldHaveAll26Upgrades()
    {
        Assert.Equal(26, UpgradeCatalog.AllUpgrades.Count);
    }

    [Fact]
    public void UpgradeCatalog_ShouldContainDoubleBarrelLaser()
    {
        var upgrade = UpgradeCatalog.Get("double_barrel_laser");
        
        Assert.NotNull(upgrade);
        Assert.Equal("Double-Barreled Laser", upgrade.Name);
        Assert.Equal(UpgradeType.Permanent, upgrade.Type);
    }

    [Fact]
    public void UpgradeCatalog_ShouldContainRearFiringLaser()
    {
        var upgrade = UpgradeCatalog.Get("rear_firing_laser");
        
        Assert.NotNull(upgrade);
        Assert.Equal("Rear-Firing Laser", upgrade.Name);
    }

    [Fact]
    public void UpgradeCatalog_ShouldContainMiniHowitzer()
    {
        var upgrade = UpgradeCatalog.Get("mini_howitzer");
        
        Assert.NotNull(upgrade);
        Assert.Equal(5, upgrade.MaxUses);
        Assert.Equal(UpgradeType.Temporary, upgrade.Type);
    }

    [Fact]
    public void UpgradeCatalog_GetUnknown_ShouldReturnNull()
    {
        var upgrade = UpgradeCatalog.Get("unknown_upgrade");
        
        Assert.Null(upgrade);
    }

    [Fact]
    public void DoubleBarrelLaser_ShouldHaveLaserStrategy()
    {
        var upgrade = new DoubleBarrelLaser();
        
        var strategy = upgrade.GetLaserStrategy();
        
        Assert.NotNull(strategy);
    }

    [Fact]
    public void DoubleBarrelLaser_LaserStrategy_ShouldReturn3Directions()
    {
        var upgrade = new DoubleBarrelLaser();
        var strategy = upgrade.GetLaserStrategy()!;
        var robot = new Robot(Direction.North);
        
        var directions = strategy.GetFireDirections(robot, Direction.North).ToList();
        
        Assert.Equal(3, directions.Count);
        Assert.Contains(Direction.North, directions);
        Assert.Contains(Direction.West, directions);
        Assert.Contains(Direction.East, directions);
    }

    [Fact]
    public void RearFiringLaser_ShouldHaveLaserStrategy()
    {
        var upgrade = new RearFiringLaser();
        
        var strategy = upgrade.GetLaserStrategy();
        
        Assert.NotNull(strategy);
        Assert.True(strategy.ShouldFireRearLaser(new Robot()));
    }

    [Fact]
    public void GyroscopicStabilizer_ShouldHaveConveyorStrategy()
    {
        var upgrade = new GyroscopicStabilizer();
        
        var strategy = upgrade.GetConveyorStrategy();
        
        Assert.NotNull(strategy);
        Assert.True(strategy.ShouldIgnoreConveyors(new Robot()));
    }

    [Fact]
    public void AblativeCoat_ShouldHaveDamageStrategy()
    {
        var upgrade = new AblativeCoat();
        
        var strategy = upgrade.GetDamageStrategy();
        
        Assert.NotNull(strategy);
    }

    [Fact]
    public void AblativeCoat_ShouldHave3Charges()
    {
        var upgrade = new AblativeCoat();
        
        Assert.Equal(3, upgrade.RemainingCharges);
    }

    [Fact]
    public void RammingGear_ShouldAddCollisionDamage()
    {
        var upgrade = new RammingGear();
        var strategy = upgrade.GetDamageStrategy()!;
        var robot = new Robot();
        
        var collisionDamage = strategy.GetCollisionDamage(robot);
        
        Assert.Equal(1, collisionDamage);
    }

    [Fact]
    public void Brakes_ShouldHaveMovementStrategy()
    {
        var upgrade = new Brakes();
        
        var strategy = upgrade.GetMovementStrategy();
        
        Assert.NotNull(strategy);
        Assert.True(strategy.ShouldStopAtObstacle(new Robot()));
    }

    [Fact]
    public void TemporaryUpgrades_ShouldHaveCorrectTypes()
    {
        Assert.Equal(UpgradeType.Temporary, new AbortSwitch().Type);
        Assert.Equal(UpgradeType.Temporary, new ConditionalProgram().Type);
        Assert.Equal(UpgradeType.Temporary, new Flywheel().Type);
        Assert.Equal(UpgradeType.Temporary, new MiniHowitzer().Type);
        Assert.Equal(UpgradeType.Temporary, new Recompile().Type);
    }

    [Fact]
    public void PermanentUpgrades_ShouldHaveCorrectTypes()
    {
        Assert.Equal(UpgradeType.Permanent, new DoubleBarrelLaser().Type);
        Assert.Equal(UpgradeType.Permanent, new RearFiringLaser().Type);
        Assert.Equal(UpgradeType.Permanent, new Brakes().Type);
    }

    [Fact]
    public void MiniHowitzer_ShouldHave5Uses()
    {
        var upgrade = new MiniHowitzer();
        
        Assert.Equal(5, upgrade.UsesRemaining);
    }

    [Fact]
    public void Recompile_ShouldHave1Use()
    {
        var upgrade = new Recompile();
        
        Assert.Equal(1, upgrade.UsesRemaining);
    }

    [Fact]
    public void Flywheel_ShouldAllowSavingCard()
    {
        var upgrade = new Flywheel();
        var card = new MoveCard { Priority = 1, Spaces = 1 };
        
        upgrade.SavedCard = card;
        
        Assert.NotNull(upgrade.SavedCard);
        Assert.Equal(card.Priority, upgrade.SavedCard.Priority);
    }

    [Fact]
    public void ConditionalProgram_ShouldAllowSavingCard()
    {
        var upgrade = new ConditionalProgram();
        var card = new MoveCard { Priority = 2, Spaces = 2 };
        
        upgrade.SavedCard = card;
        
        Assert.NotNull(upgrade.SavedCard);
        Assert.Equal(1, upgrade.UsesRemaining);
    }

    [Fact]
    public void Player_ShouldHaveUpgradeLists()
    {
        var player = new Player();
        
        Assert.NotNull(player.PermanentUpgrades);
        Assert.NotNull(player.TemporaryUpgrades);
        Assert.Empty(player.PermanentUpgrades);
        Assert.Empty(player.TemporaryUpgrades);
    }

    [Fact]
    public void Player_AddUpgrade_ShouldAddToCorrectList()
    {
        var player = new Player();
        var permanent = new DoubleBarrelLaser();
        var temporary = new Recompile();
        
        player.AddUpgrade(permanent);
        player.AddUpgrade(temporary);
        
        Assert.Single(player.PermanentUpgrades);
        Assert.Single(player.TemporaryUpgrades);
    }

    [Fact]
    public void Player_RemoveUpgrade_ShouldRemoveFromCorrectList()
    {
        var player = new Player();
        var permanent = new DoubleBarrelLaser();
        
        player.AddUpgrade(permanent);
        player.RemoveUpgrade(permanent);
        
        Assert.Empty(player.PermanentUpgrades);
    }

    [Fact]
    public void Player_ClearTemporaryUpgrades_ShouldRemoveOnlyTemporary()
    {
        var player = new Player();
        var permanent = new DoubleBarrelLaser();
        var temporary = new Recompile();
        
        player.AddUpgrade(permanent);
        player.AddUpgrade(temporary);
        player.ClearTemporaryUpgrades();
        
        Assert.Single(player.PermanentUpgrades);
        Assert.Empty(player.TemporaryUpgrades);
    }

    [Fact]
    public void Player_AllUpgrades_ShouldCombineBothLists()
    {
        var player = new Player();
        var permanent = new DoubleBarrelLaser();
        var temporary = new Recompile();
        
        player.AddUpgrade(permanent);
        player.AddUpgrade(temporary);
        
        var all = player.AllUpgrades.ToList();
        
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void UpgradeEffects_RegisterAll_ShouldRegisterAll26Upgrades()
    {
        UpgradeEffects.RegisterAll();
        
        Assert.Equal(26, UpgradeRegistry.All.Count);
    }

    [Fact]
    public void UpgradeRegistry_Create_ShouldReturnCorrectType()
    {
        UpgradeEffects.RegisterAll();
        
        var upgrade = UpgradeRegistry.Create("double_barrel_laser");
        
        Assert.NotNull(upgrade);
        Assert.IsType<DoubleBarrelLaser>(upgrade);
    }

    [Fact]
    public void UpgradeRegistry_CreateFromDefinition_ShouldWork()
    {
        UpgradeEffects.RegisterAll();
        var definition = UpgradeCatalog.DoubleBarrelLaser;
        
        var effect = UpgradeCatalog.CreateEffect(definition);
        
        Assert.NotNull(effect);
        Assert.IsType<DoubleBarrelLaser>(effect);
    }

    [Fact]
    public void CompositeLaserStrategy_ShouldCombineStrategies()
    {
        var strategies = new List<ILaserStrategy>
        {
            new DoubleBarrelLaser().GetLaserStrategy()!,
            new RearFiringLaser().GetLaserStrategy()!
        };
        
        var composite = new CompositeLaserStrategy(strategies);
        var robot = new Robot(Direction.North);
        
        var directions = composite.GetFireDirections(robot, Direction.North).ToList();
        var fireRear = composite.ShouldFireRearLaser(robot);
        
        Assert.True(directions.Count >= 3);
        Assert.True(fireRear);
    }

    [Fact]
    public void CompositeConveyorStrategy_ShouldCombineStrategies()
    {
        var strategies = new List<IConveyorStrategy>
        {
            new GyroscopicStabilizer().GetConveyorStrategy()!
        };
        
        var composite = new CompositeConveyorStrategy(strategies);
        var robot = new Robot();
        
        Assert.True(composite.ShouldIgnoreConveyors(robot));
    }

    [Fact]
    public void UpgradeContext_ShouldStoreMetadata()
    {
        var context = new UpgradeContext
        {
            Robot = new Robot { Id = 1 },
            Metadata = new Dictionary<string, object>()
        };
        
        context.Metadata["TestKey"] = "TestValue";
        
        Assert.Equal("TestValue", context.Metadata["TestKey"]);
    }

    [Fact]
    public void Brakes_PlayerWithUpgrade_StopsAtWall()
    {
        var moveCard = new MoveCard { Priority = 100, Spaces = 3 };
        var player = CreateWithHand(moveCard);
        player.Robot.SetPosition(new Position(2, 2));
        player.Robot.SetDirection(Direction.East);
        player.AssignCardToRegister(0, 0);
        player.AddUpgrade(new Brakes());
        
        var engine = CreateEngine(new GameState(new List<Player> { player }));
        engine.State.Map.SetTile(4, 2, TileFactory.Wall());
        
        ExecuteRegister(engine, 0);
        
        Assert.Equal(new Position(3, 2), player.Robot.Position);
    }

    [Fact]
    public void GyroscopicStabilizer_PlayerWithUpgrade_IgnoresConveyor()
    {
        var moveCard = new MoveCard { Priority = 100, Spaces = 1 };
        var player = CreateWithHand(moveCard);
        player.Robot.SetPosition(new Position(2, 2));
        player.Robot.SetDirection(Direction.East);
        player.AssignCardToRegister(0, 0);
        player.AddUpgrade(new GyroscopicStabilizer());
        
        var engine = CreateEngine(new GameState(new List<Player> { player }));
        engine.State.Map.SetTile(3, 2, TileFactory.Conveyor(ConveyorDirection.East));
        
        ExecuteRegister(engine, 0);
        
        Assert.Equal(new Position(3, 2), player.Robot.Position);
    }

    [Fact]
    public void All26Upgrades_ShouldHaveDescriptions()
    {
        foreach (var upgrade in UpgradeCatalog.AllUpgrades)
        {
            Assert.False(string.IsNullOrEmpty(upgrade.Description), 
                $"Upgrade {upgrade.Id} should have a description");
        }
    }

    private static GameEngine CreateEngine(GameState state)
    {
        var engine = new GameEngine(new RandomDealer(), state);
        var map = new GameMap(10, 10);
        map.SetTile(1, 1, TileFactory.Start());
        map.SetTile(8, 1, TileFactory.Start());
        engine.Initialize(map);
        return engine;
    }
    
    private static GameEngine CreateEngine()
    {
        return CreateEngine(new GameState());
    }
}
