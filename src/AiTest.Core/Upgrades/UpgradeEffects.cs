namespace AiTest.Core.Upgrades;

using AiTest.Core.Entities;
using AiTest.Core.Models;

public static class StandardUpgrades
{
    public sealed class DoubleBarrelLaser : UpgradeEffect
    {
        public override string Name => "Double Barrel Laser";
        public override UpgradeType Type => UpgradeType.Permanent;
        
        public override ILaserStrategy GetLaserStrategy() => new DoubleBarrelLaserStrategy();
        
        private class DoubleBarrelLaserStrategy : ILaserStrategy
        {
            public IEnumerable<Direction> GetFireDirections(Robot robot, Direction defaultDirection)
            {
                return new[] { defaultDirection, defaultDirection.TurnLeft(), defaultDirection.TurnRight() };
            }
            
            public int GetDamageBonus(Robot robot) => 0;
            public bool ShouldFireRearLaser(Robot robot) => false;
        }
    }
    
    public sealed class RearLaser : UpgradeEffect
    {
        public override string Name => "Rear Laser";
        public override UpgradeType Type => UpgradeType.Permanent;
        
        public override ILaserStrategy GetLaserStrategy() => new RearLaserStrategy();
        
        private class RearLaserStrategy : ILaserStrategy
        {
            public IEnumerable<Direction> GetFireDirections(Robot robot, Direction defaultDirection)
                => new[] { defaultDirection };
            
            public int GetDamageBonus(Robot robot) => 0;
            public bool ShouldFireRearLaser(Robot robot) => true;
        }
    }
    
    public sealed class HoverUnit : UpgradeEffect
    {
        public override string Name => "Hover Unit";
        public override UpgradeType Type => UpgradeType.Permanent;
        
        public override IConveyorStrategy GetConveyorStrategy() => new HoverUnitStrategy();
        
        private class HoverUnitStrategy : IConveyorStrategy
        {
            public bool ShouldIgnoreConveyors(Robot robot) => true;
            public bool ShouldIgnorePits(Robot robot) => true;
        }
    }
    
    public sealed class PowerSlide : UpgradeEffect
    {
        public override string Name => "Power Slide";
        public override UpgradeType Type => UpgradeType.Permanent;
        
        public override IMovementStrategy GetMovementStrategy() => new PowerSlideStrategy();
        
        private class PowerSlideStrategy : IMovementStrategy
        {
            public Position CalculateMove(Robot robot, GameMap map, Position target) => target;
            public bool CanEnterTile(Robot robot, GameMap map, Position pos) => true;
            public bool ShouldStopAtObstacle(Robot robot) => false;
            public int GetExtraMoveSpaces(Robot robot) => 1;
        }
    }
    
    public sealed class Drifting : UpgradeEffect
    {
        public override string Name => "Drifting";
        public override UpgradeType Type => UpgradeType.Permanent;
        
        public override IMovementStrategy GetMovementStrategy() => new DriftingStrategy();
        
        private class DriftingStrategy : IMovementStrategy
        {
            public Position CalculateMove(Robot robot, GameMap map, Position target) => target;
            public bool CanEnterTile(Robot robot, GameMap map, Position pos) => true;
            public bool ShouldStopAtObstacle(Robot robot) => false;
            public int GetExtraMoveSpaces(Robot robot) => 0;
        }
    }
    
    public sealed class Spiky : UpgradeEffect
    {
        public override string Name => "Spiky";
        public override UpgradeType Type => UpgradeType.Permanent;
        
        public override IDamageStrategy GetDamageStrategy() => new SpikyStrategy();
        
        private class SpikyStrategy : IDamageStrategy
        {
            public int CalculateDamage(Robot robot, int baseDamage) => baseDamage;
            public bool CanDeflect(Robot robot) => false;
            public bool CanBounceBack(Robot robot, out int bounceDistance)
            {
                bounceDistance = 0;
                return false;
            }
            public int GetCollisionDamage(Robot robot) => 1;
        }
    }
    
    public sealed class DeflectorShield : UpgradeEffect
    {
        public override string Name => "Deflector Shield";
        public override UpgradeType Type => UpgradeType.Permanent;
        
        public override IDamageStrategy GetDamageStrategy() => new DeflectorShieldStrategy();
        
        private class DeflectorShieldStrategy : IDamageStrategy
        {
            public int CalculateDamage(Robot robot, int baseDamage) => Math.Max(0, baseDamage - 1);
            public bool CanDeflect(Robot robot) => true;
            public bool CanBounceBack(Robot robot, out int bounceDistance)
            {
                bounceDistance = 0;
                return false;
            }
            public int GetCollisionDamage(Robot robot) => 0;
        }
    }
    
    public sealed class Brakes : UpgradeEffect
    {
        public override string Name => "Brakes";
        public override UpgradeType Type => UpgradeType.Permanent;
        
        public override IMovementStrategy GetMovementStrategy() => new BrakesStrategy();
        
        private class BrakesStrategy : IMovementStrategy
        {
            public Position CalculateMove(Robot robot, GameMap map, Position target) => target;
            public bool CanEnterTile(Robot robot, GameMap map, Position pos) => true;
            public bool ShouldStopAtObstacle(Robot robot) => true;
            public int GetExtraMoveSpaces(Robot robot) => 0;
        }
    }
    
    public sealed class Boink : UpgradeEffect
    {
        public override string Name => "Boink";
        public override UpgradeType Type => UpgradeType.Temporary;
        
        public override IDamageStrategy GetDamageStrategy() => new BoinkStrategy();
        
        private class BoinkStrategy : IDamageStrategy
        {
            public int CalculateDamage(Robot robot, int baseDamage) => 0;
            public bool CanDeflect(Robot robot) => false;
            public bool CanBounceBack(Robot robot, out int bounceDistance)
            {
                bounceDistance = 2;
                return true;
            }
            public int GetCollisionDamage(Robot robot) => 0;
        }
    }
    
    public sealed class Overclocked : UpgradeEffect
    {
        public override string Name => "Overclocked";
        public override UpgradeType Type => UpgradeType.Temporary;
        
        public override IMovementStrategy GetMovementStrategy() => new OverclockedMovementStrategy();
        public override ICardExecutionStrategy GetCardExecutionStrategy() => new OverclockedCardStrategy();
        
        private class OverclockedMovementStrategy : IMovementStrategy
        {
            public Position CalculateMove(Robot robot, GameMap map, Position target) => target;
            public bool CanEnterTile(Robot robot, GameMap map, Position pos) => true;
            public bool ShouldStopAtObstacle(Robot robot) => false;
            public int GetExtraMoveSpaces(Robot robot) => 0;
        }
        
        private class OverclockedCardStrategy : ICardExecutionStrategy
        {
            public void ExecuteRotation(Robot robot) { }
            public int GetExtraMoveSpaces(Robot robot, int baseSpaces) => 0;
            public bool ShouldRepeatLastRegister(Robot robot) => true;
        }
    }
    
    public static void RegisterAll()
    {
        UpgradeRegistry.Register("Double Barrel Laser", () => new DoubleBarrelLaser());
        UpgradeRegistry.Register("Rear Laser", () => new RearLaser());
        UpgradeRegistry.Register("Hover Unit", () => new HoverUnit());
        UpgradeRegistry.Register("Power Slide", () => new PowerSlide());
        UpgradeRegistry.Register("Drifting", () => new Drifting());
        UpgradeRegistry.Register("Spiky", () => new Spiky());
        UpgradeRegistry.Register("Deflector Shield", () => new DeflectorShield());
        UpgradeRegistry.Register("Brakes", () => new Brakes());
        UpgradeRegistry.Register("Boink", () => new Boink());
        UpgradeRegistry.Register("Overclocked", () => new Overclocked());
    }
}
