namespace RoboRallyGame.Core.Upgrades
{
    using RoboRallyGame.Core.Entities;
    using RoboRallyGame.Core.Models;
    using RoboRallyGame.Core.Cards;

    public static class UpgradeEffects
    {
        public static void RegisterAll()
        {
            UpgradeRegistry.Register("ablative_coat", () => new Permanent.AblativeCoat());
            UpgradeRegistry.Register("abort_switch", () => new Temporary.AbortSwitch());
            UpgradeRegistry.Register("brakes", () => new Permanent.Brakes());
            UpgradeRegistry.Register("circuit_breaker", () => new Permanent.CircuitBreaker());
            UpgradeRegistry.Register("conditional_program", () => new Temporary.ConditionalProgram());
            UpgradeRegistry.Register("crab_legs", () => new Permanent.CrabLegs());
            UpgradeRegistry.Register("double_barrel_laser", () => new Permanent.DoubleBarrelLaser());
            UpgradeRegistry.Register("dual_processor", () => new Permanent.DualProcessor());
            UpgradeRegistry.Register("extra_memory", () => new Permanent.ExtraMemory());
            UpgradeRegistry.Register("fire_control", () => new Permanent.FireControl());
            UpgradeRegistry.Register("flywheel", () => new Temporary.Flywheel());
            UpgradeRegistry.Register("fourth_gear", () => new Permanent.FourthGear());
            UpgradeRegistry.Register("gyroscopic_stabilizer", () => new Permanent.GyroscopicStabilizer());
            UpgradeRegistry.Register("high_power_laser", () => new Permanent.HighPowerLaser());
            UpgradeRegistry.Register("mechanical_arm", () => new Permanent.MechanicalArm());
            UpgradeRegistry.Register("mini_howitzer", () => new Temporary.MiniHowitzer());
            UpgradeRegistry.Register("power_down_shield", () => new Permanent.PowerDownShield());
            UpgradeRegistry.Register("pressor_beam", () => new Permanent.PressorBeam());
            UpgradeRegistry.Register("radio_control", () => new Permanent.RadioControl());
            UpgradeRegistry.Register("ramming_gear", () => new Permanent.RammingGear());
            UpgradeRegistry.Register("rear_firing_laser", () => new Permanent.RearFiringLaser());
            UpgradeRegistry.Register("recompile", () => new Temporary.Recompile());
            UpgradeRegistry.Register("reverse_gear", () => new Permanent.ReverseGear());
            UpgradeRegistry.Register("scrambler", () => new Permanent.Scrambler());
            UpgradeRegistry.Register("superior_archive", () => new Permanent.SuperiorArchive());
            UpgradeRegistry.Register("tractor_beam", () => new Permanent.TractorBeam());
        }
    }

    namespace Permanent
    {
        public sealed class AblativeCoat : UpgradeEffect
        {
            public override string Name => "Ablative Coat";
            public override UpgradeType Type => UpgradeType.Permanent;
            public override IDamageStrategy? GetDamageStrategy() => new AblativeCoatStrategy();
            public int RemainingCharges { get; set; } = 3;

            private class AblativeCoatStrategy : IDamageStrategy
            {
                public int CalculateDamage(Robot robot, int baseDamage) => baseDamage;
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
            public override IMovementStrategy? GetMovementStrategy() => new BrakesStrategy();

            private class BrakesStrategy : IMovementStrategy
            {
                public Position CalculateMove(Robot robot, GameMap map, Position target) => target;
                public bool CanEnterTile(Robot robot, GameMap map, Position pos) => true;
                public bool ShouldStopAtObstacle(Robot robot) => true;
                public int GetExtraMoveSpaces(Robot robot) => 0;
            }
        }

        public sealed class CircuitBreaker : UpgradeEffect
        {
            public override string Name => "Circuit Breaker";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class CrabLegs : UpgradeEffect
        {
            public override string Name => "Crab Legs";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class DoubleBarrelLaser : UpgradeEffect
        {
            public override string Name => "Double-Barreled Laser";
            public override UpgradeType Type => UpgradeType.Permanent;
            public override ILaserStrategy? GetLaserStrategy() => new DoubleBarrelLaserStrategy();

            private class DoubleBarrelLaserStrategy : ILaserStrategy
            {
                public IEnumerable<Direction> GetFireDirections(Robot robot, Direction defaultDirection)
                {
                    yield return defaultDirection;
                    yield return defaultDirection.TurnLeft();
                    yield return defaultDirection.TurnRight();
                }

                public int GetDamageBonus(Robot robot) => 0;
                public bool ShouldFireRearLaser(Robot robot) => false;
            }
        }

        public sealed class DualProcessor : UpgradeEffect
        {
            public override string Name => "Dual Processor";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class ExtraMemory : UpgradeEffect
        {
            public override string Name => "Extra Memory";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class FireControl : UpgradeEffect
        {
            public override string Name => "Fire Control";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class FourthGear : UpgradeEffect
        {
            public override string Name => "Fourth Gear";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class GyroscopicStabilizer : UpgradeEffect
        {
            public override string Name => "Gyroscopic Stabilizer";
            public override UpgradeType Type => UpgradeType.Permanent;
            public override IConveyorStrategy? GetConveyorStrategy() => new GyroStabilizerStrategy();

            private class GyroStabilizerStrategy : IConveyorStrategy
            {
                public bool ShouldIgnoreConveyors(Robot robot) => true;
                public bool ShouldIgnorePits(Robot robot) => false;
            }
        }

        public sealed class HighPowerLaser : UpgradeEffect
        {
            public override string Name => "High-power Laser";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class MechanicalArm : UpgradeEffect
        {
            public override string Name => "Mechanical Arm";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class PowerDownShield : UpgradeEffect
        {
            public override string Name => "Power-down Shield";
            public override UpgradeType Type => UpgradeType.Permanent;
            public override IDamageStrategy? GetDamageStrategy() => new PowerDownShieldStrategy();

            private class PowerDownShieldStrategy : IDamageStrategy
            {
                public int CalculateDamage(Robot robot, int baseDamage) 
                    => robot.IsPowerDown ? Math.Max(0, baseDamage - 1) : baseDamage;
                public bool CanDeflect(Robot robot) => false;
                public bool CanBounceBack(Robot robot, out int bounceDistance)
                {
                    bounceDistance = 0;
                    return false;
                }
                public int GetCollisionDamage(Robot robot) => 0;
            }
        }

        public sealed class PressorBeam : UpgradeEffect
        {
            public override string Name => "Pressor Beam";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class RadioControl : UpgradeEffect
        {
            public override string Name => "Radio Control";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class RammingGear : UpgradeEffect
        {
            public override string Name => "Ramming Gear";
            public override UpgradeType Type => UpgradeType.Permanent;
            public override IDamageStrategy? GetDamageStrategy() => new RammingGearStrategy();

            private class RammingGearStrategy : IDamageStrategy
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

        public sealed class RearFiringLaser : UpgradeEffect
        {
            public override string Name => "Rear-Firing Laser";
            public override UpgradeType Type => UpgradeType.Permanent;
            public override ILaserStrategy? GetLaserStrategy() => new RearFiringLaserStrategy();

            private class RearFiringLaserStrategy : ILaserStrategy
            {
                public IEnumerable<Direction> GetFireDirections(Robot robot, Direction defaultDirection)
                    => new[] { defaultDirection };

                public int GetDamageBonus(Robot robot) => 0;
                public bool ShouldFireRearLaser(Robot robot) => true;
            }
        }

        public sealed class ReverseGear : UpgradeEffect
        {
            public override string Name => "Reverse Gear";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class Scrambler : UpgradeEffect
        {
            public override string Name => "Scrambler";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class SuperiorArchive : UpgradeEffect
        {
            public override string Name => "Superior Archive";
            public override UpgradeType Type => UpgradeType.Permanent;
        }

        public sealed class TractorBeam : UpgradeEffect
        {
            public override string Name => "Tractor Beam";
            public override UpgradeType Type => UpgradeType.Permanent;
        }
    }

    namespace Temporary
    {
        public sealed class AbortSwitch : UpgradeEffect
        {
            public override string Name => "Abort Switch";
            public override UpgradeType Type => UpgradeType.Temporary;
            public int UsesRemaining { get; set; } = 1;
        }

        public sealed class ConditionalProgram : UpgradeEffect
        {
            public override string Name => "Conditional Program";
            public override UpgradeType Type => UpgradeType.Temporary;
            public ProgramCard? SavedCard { get; set; }
            public int UsesRemaining { get; set; } = 1;
        }

        public sealed class Flywheel : UpgradeEffect
        {
            public override string Name => "Flywheel";
            public override UpgradeType Type => UpgradeType.Temporary;
            public ProgramCard? SavedCard { get; set; }
        }

        public sealed class MiniHowitzer : UpgradeEffect
        {
            public override string Name => "Mini Howitzer";
            public override UpgradeType Type => UpgradeType.Temporary;
            public int UsesRemaining { get; set; } = 5;
        }

        public sealed class Recompile : UpgradeEffect
        {
            public override string Name => "Recompile";
            public override UpgradeType Type => UpgradeType.Temporary;
            public int UsesRemaining { get; set; } = 1;
        }
    }
}
