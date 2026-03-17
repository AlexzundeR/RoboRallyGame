namespace AiTest.Core.Tests.Upgrades;

using AiTest.Core.Upgrades;
using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Engine;

public class UpgradeSystemTests
{
    [Fact]
    public void UpgradeEffect_ShouldHaveCorrectName()
    {
        var upgrade = new StandardUpgrades.DoubleBarrelLaser();
        
        Assert.Equal("Double Barrel Laser", upgrade.Name);
    }

    [Fact]
    public void UpgradeEffect_ShouldHaveCorrectType()
    {
        var permanent = new StandardUpgrades.DoubleBarrelLaser();
        var temporary = new StandardUpgrades.Boink();
        
        Assert.Equal(UpgradeType.Permanent, permanent.Type);
        Assert.Equal(UpgradeType.Temporary, temporary.Type);
    }

    [Fact]
    public void UpgradeEffect_ShouldProvideLaserStrategy()
    {
        var upgrade = new StandardUpgrades.DoubleBarrelLaser();
        
        var strategy = upgrade.GetLaserStrategy();
        
        Assert.NotNull(strategy);
    }

    [Fact]
    public void UpgradeEffect_ShouldProvideConveyorStrategy()
    {
        var upgrade = new StandardUpgrades.HoverUnit();
        
        var strategy = upgrade.GetConveyorStrategy();
        
        Assert.NotNull(strategy);
        Assert.True(strategy.ShouldIgnoreConveyors(new Robot()));
        Assert.True(strategy.ShouldIgnorePits(new Robot()));
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
    public void Player_ShouldHaveUpgradeLists()
    {
        var player = new Player { Id = 1 };
        
        Assert.NotNull(player.PermanentUpgrades);
        Assert.NotNull(player.TemporaryUpgrades);
        Assert.Empty(player.PermanentUpgrades);
        Assert.Empty(player.TemporaryUpgrades);
    }

    [Fact]
    public void Player_AddUpgrade_ShouldAddToCorrectList()
    {
        var player = new Player { Id = 1 };
        var permanent = new StandardUpgrades.DoubleBarrelLaser();
        var temporary = new StandardUpgrades.Boink();
        
        player.AddUpgrade(permanent);
        player.AddUpgrade(temporary);
        
        Assert.Single(player.PermanentUpgrades);
        Assert.Single(player.TemporaryUpgrades);
    }

    [Fact]
    public void Player_RemoveUpgrade_ShouldRemoveFromCorrectList()
    {
        var player = new Player { Id = 1 };
        var permanent = new StandardUpgrades.DoubleBarrelLaser();
        
        player.AddUpgrade(permanent);
        player.RemoveUpgrade(permanent);
        
        Assert.Empty(player.PermanentUpgrades);
    }

    [Fact]
    public void Player_ClearTemporaryUpgrades_ShouldRemoveOnlyTemporary()
    {
        var player = new Player { Id = 1 };
        var permanent = new StandardUpgrades.DoubleBarrelLaser();
        var temporary = new StandardUpgrades.Boink();
        
        player.AddUpgrade(permanent);
        player.AddUpgrade(temporary);
        player.ClearTemporaryUpgrades();
        
        Assert.Single(player.PermanentUpgrades);
        Assert.Empty(player.TemporaryUpgrades);
    }

    [Fact]
    public void Player_AllUpgrades_ShouldCombineBothLists()
    {
        var player = new Player { Id = 1 };
        var permanent = new StandardUpgrades.DoubleBarrelLaser();
        var temporary = new StandardUpgrades.Boink();
        
        player.AddUpgrade(permanent);
        player.AddUpgrade(temporary);
        
        var all = player.AllUpgrades.ToList();
        
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void UpgradeRegistry_ShouldRegisterAndCreate()
    {
        StandardUpgrades.RegisterAll();
        
        var upgrade = UpgradeRegistry.Create("Double Barrel Laser");
        
        Assert.NotNull(upgrade);
        Assert.IsType<StandardUpgrades.DoubleBarrelLaser>(upgrade);
    }

    [Fact]
    public void UpgradeRegistry_ShouldReturnNullForUnknown()
    {
        var upgrade = UpgradeRegistry.Create("Unknown Upgrade");
        
        Assert.Null(upgrade);
    }

    [Fact]
    public void DoubleBarrelLaser_ShouldProvideLaserStrategy()
    {
        var upgrade = new StandardUpgrades.DoubleBarrelLaser();
        var strategy = upgrade.GetLaserStrategy();
        var robot = new Robot { Direction = Direction.North };
        
        var directions = strategy.GetFireDirections(robot, Direction.North).ToList();
        
        Assert.Contains(Direction.North, directions);
        Assert.Contains(Direction.West, directions);
        Assert.Contains(Direction.East, directions);
    }

    [Fact]
    public void HoverUnit_ShouldProvideConveyorStrategy()
    {
        var upgrade = new StandardUpgrades.HoverUnit();
        var strategy = upgrade.GetConveyorStrategy();
        var robot = new Robot();
        
        Assert.True(strategy.ShouldIgnoreConveyors(robot));
        Assert.True(strategy.ShouldIgnorePits(robot));
    }

    [Fact]
    public void Spiky_ShouldProvideDamageStrategy()
    {
        var upgrade = new StandardUpgrades.Spiky();
        var strategy = upgrade.GetDamageStrategy();
        var robot = new Robot();
        
        Assert.Equal(1, strategy.GetCollisionDamage(robot));
    }

    [Fact]
    public void Boink_ShouldProvideDamageStrategy()
    {
        var upgrade = new StandardUpgrades.Boink();
        var strategy = upgrade.GetDamageStrategy();
        var robot = new Robot();
        
        var canBounce = strategy.CanBounceBack(robot, out var distance);
        
        Assert.True(canBounce);
        Assert.Equal(2, distance);
    }

    [Fact]
    public void AllStandardUpgrades_ShouldBeRegistered()
    {
        StandardUpgrades.RegisterAll();
        
        var upgradeNames = new[] 
        { 
            "Double Barrel Laser", "Rear Laser", "Hover Unit", 
            "Power Slide", "Drifting", "Spiky", "Deflector Shield",
            "Brakes", "Boink", "Overclocked" 
        };
        
        foreach (var name in upgradeNames)
        {
            var upgrade = UpgradeRegistry.Create(name);
            Assert.NotNull(upgrade);
        }
    }

    [Fact]
    public void CompositeLaserStrategy_ShouldCombineStrategies()
    {
        var strategies = new List<ILaserStrategy>
        {
            new StandardUpgrades.DoubleBarrelLaser().GetLaserStrategy()!,
            new StandardUpgrades.RearLaser().GetLaserStrategy()!
        };
        
        var composite = new CompositeLaserStrategy(strategies);
        var robot = new Robot { Direction = Direction.North };
        
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
            new StandardUpgrades.HoverUnit().GetConveyorStrategy()!
        };
        
        var composite = new CompositeConveyorStrategy(strategies);
        var robot = new Robot();
        
        Assert.True(composite.ShouldIgnoreConveyors(robot));
        Assert.True(composite.ShouldIgnorePits(robot));
    }

    [Fact]
    public void PowerSlide_PlayerWithUpgrade_MovesExtraSpace()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        
        var player = engine.State.Players[0];
        player.AddUpgrade(new StandardUpgrades.PowerSlide());
        
        player.Robot.Position = new Position(2, 2);
        player.Robot.Direction = Direction.East;
        
        var moveCard = new MoveCard { Priority = 100, Spaces = 1 };
        player.SetRegister(0, moveCard);
        
        engine.ExecuteRegister(0);
        
        Assert.Equal(new Position(4, 2), player.Robot.Position);
    }

    [Fact]
    public void Brakes_PlayerWithUpgrade_StopsAtWall()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(4, 2, Tile.Wall);
        
        engine.AddPlayer("Player1");
        
        var player = engine.State.Players[0];
        player.AddUpgrade(new StandardUpgrades.Brakes());
        
        player.Robot.Position = new Position(2, 2);
        player.Robot.Direction = Direction.East;
        
        var moveCard = new MoveCard { Priority = 100, Spaces = 3 };
        player.SetRegister(0, moveCard);
        
        engine.ExecuteRegister(0);
        
        Assert.Equal(new Position(3, 2), player.Robot.Position);
    }

    [Fact]
    public void HoverUnit_PlayerWithUpgrade_IgnoresConveyor()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(3, 2, Tile.Conveyor(ConveyorDirection.East));
        
        engine.AddPlayer("Player1");
        
        var player = engine.State.Players[0];
        player.AddUpgrade(new StandardUpgrades.HoverUnit());
        
        player.Robot.Position = new Position(2, 2);
        player.Robot.Direction = Direction.East;
        
        var moveCard = new MoveCard { Priority = 100, Spaces = 1 };
        player.SetRegister(0, moveCard);
        
        engine.ExecuteRegister(0);
        
        Assert.Equal(new Position(3, 2), player.Robot.Position);
    }

    [Fact]
    public void HoverUnit_PlayerWithUpgrade_IgnoresPit()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(3, 2, Tile.Pit);
        
        engine.AddPlayer("Player1");
        
        var player = engine.State.Players[0];
        player.AddUpgrade(new StandardUpgrades.HoverUnit());
        
        player.Robot.Position = new Position(3, 2);
        player.Robot.Direction = Direction.East;
        
        engine.ProcessBoardElements();
        
        Assert.Equal(RobotStatus.Active, player.Robot.Status);
    }

    [Fact]
    public void DeflectorShield_PlayerWithUpgrade_ProvidesDeflect()
    {
        var upgrade = new StandardUpgrades.DeflectorShield();
        var strategy = upgrade.GetDamageStrategy()!;
        var robot = new Robot();
        
        var damage = strategy.CalculateDamage(robot, 2);
        
        Assert.Equal(1, damage);
        Assert.True(strategy.CanDeflect(robot));
    }

    [Fact]
    public void Overclocked_PlayerWithUpgrade_ProvidesCardExecutionStrategy()
    {
        var upgrade = new StandardUpgrades.Overclocked();
        var strategy = upgrade.GetCardExecutionStrategy()!;
        var robot = new Robot();
        
        Assert.True(strategy.ShouldRepeatLastRegister(robot));
    }

    private static GameEngine CreateEngine()
    {
        var hand = new List<ProgramCard>
        {
            new MoveCard { Priority = 100, Spaces = 1 },
            new MoveCard { Priority = 101, Spaces = 2 },
            new MoveCard { Priority = 102, Spaces = 3 },
            new TurnCard { Priority = 150, IsLeft = true },
            new TurnCard { Priority = 151, IsLeft = false },
            new UTurnCard { Priority = 350 },
            new MoveCard { Priority = 103, Spaces = 1 },
            new MoveCard { Priority = 104, Spaces = 2 },
            new MoveCard { Priority = 105, Spaces = 3 },
        };
        
        var dealer = new FixedDealer(hand);
        var engine = new GameEngine(dealer);
        var map = new GameMap(10, 10);
        map.SetTile(1, 1, Tile.Start);
        map.SetTile(8, 1, Tile.Start);
        engine.Initialize(map);
        return engine;
    }
}
