namespace AiTest.Core.Tests.Engine;

using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Models.Components;

public class CollisionResolverTests
{
    [Fact]
    public void Resolve_TwoRobotsMovingToSameEmptySpace_HigherPriorityWins()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(7, 5), Direction.West);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 100,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot2,
            From = new Position(7, 5),
            To = new Position(6, 5),
            Direction = Direction.West,
            Priority = 200,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1, robot2 });
        
        Assert.Equal(new Position(6, 5), robot2.Position);
    }

    [Fact]
    public void Resolve_PushRobot_MovesPushedRobot()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(6, 5), Direction.North);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 100,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1, robot2 });
        
        Assert.Equal(new Position(7, 5), robot2.Position);
    }

    [Fact]
    public void Resolve_PushIntoWall_Blocked()
    {
        var map = new GameMap(10, 10);
        map.SetTile(7, 5, TileFactory.Wall());
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(6, 5), Direction.North);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 100,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1, robot2 });
        
        Assert.Equal(new Position(5, 5), robot1.Position);
    }

    [Fact]
    public void Resolve_HigherPriorityMovesFirst()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(7, 5), Direction.West);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 300,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot2,
            From = new Position(7, 5),
            To = new Position(6, 5),
            Direction = Direction.West,
            Priority = 100,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1, robot2 });
        
        Assert.Equal(new Position(6, 5), robot1.Position);
    }

    [Fact]
    public void Resolve_MoveIntoPit_FallsIntoPit()
    {
        var map = new GameMap(10, 10);
        map.SetTile(6, 5, TileFactory.Pit());
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 100,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1 });
        
        Assert.Equal(new Position(6, 5), robot1.Position);
    }

    [Fact]
    public void Resolve_ChainPush_TwoRobotsPushed()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(6, 5), Direction.East);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 100,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1, robot2 });
        
        Assert.Equal(new Position(6, 5), robot1.Position);
        Assert.Equal(new Position(7, 5), robot2.Position);
    }

    [Fact]
    public void Resolve_TwoRobotsMovingToSamePosition_HigherPriorityWinsAndPushes()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(7, 5), Direction.West);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 100,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot2,
            From = new Position(7, 5),
            To = new Position(6, 5),
            Direction = Direction.West,
            Priority = 200,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1, robot2 });
        
        Assert.Equal(new Position(5, 5), robot1.Position);
        Assert.Equal(new Position(6, 5), robot2.Position);
    }

    [Fact]
    public void Resolve_HigherPriorityPushesLowerPriority_IntoFreeSpace()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(6, 5), Direction.North);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 200,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1, robot2 });
        
        Assert.Equal(new Position(6, 5), robot1.Position);
        Assert.Equal(new Position(7, 5), robot2.Position);
    }

    [Fact]
    public void Resolve_LowerPriorityTriesToMoveIntoHigherPriority_Blocked()
    {
        var map = new GameMap(10, 10);
        map.SetTile(7, 5, TileFactory.Wall());
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(6, 5), Direction.North);
        
        var resolver = new CollisionResolver();
        resolver.AddIntent(new MoveIntent
        {
            Robot = robot1,
            From = new Position(5, 5),
            To = new Position(6, 5),
            Direction = Direction.East,
            Priority = 100,
            RegisterIndex = 0,
            Type = MoveType.Normal
        });
        
        resolver.Resolve(map, new List<Robot> { robot1, robot2 });
        
        Assert.Equal(new Position(5, 5), robot1.Position);
        Assert.Equal(new Position(6, 5), robot2.Position);
    }
}

public class LaserResolverTests
{
    [Fact]
    public void PrepareShots_RobotWithLaser_ShouldCreateShot()
    {
        var map = new GameMap(10, 10);
        var robot = new Robot(1, new Position(5, 5), Direction.East);
        var player = new Player { Id = 1, Robot = robot };
        
        var resolver = new LaserResolver();
        var shots = resolver.PrepareShots(map, new List<Player> { player });
        
        Assert.Single(shots);
        Assert.Equal(Direction.East, shots[0].Direction);
    }

    [Fact]
    public void PrepareShots_TwoRobots_ShouldCreateTwoShots()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(5, 7), Direction.North);
        
        var resolver = new LaserResolver();
        var shots = resolver.PrepareShots(map, new List<Player> 
        { 
            new Player { Id = 1, Robot = robot1 },
            new Player { Id = 2, Robot = robot2 }
        });
        
        Assert.Equal(2, shots.Count);
    }

    [Fact]
    public void PrepareShots_OnBoardLaser_ShouldAddBoardLaser()
    {
        var map = new GameMap(10, 10);
        map.SetTile(5, 5, TileFactory.Laser(Direction.North));
        var robot = new Robot(1, new Position(5, 5), Direction.East);
        
        var resolver = new LaserResolver();
        var shots = resolver.PrepareShots(map, new List<Player> { new Player { Id = 1, Robot = robot } });
        
        Assert.Equal(2, shots.Count);
        Assert.False(shots[0].IsRobotLaser);
        Assert.True(shots[1].IsRobotLaser);
    }

    [Fact]
    public void Fire_ShotsHitTarget_ShouldDamage()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(7, 5), Direction.North);
        
        var shots = new List<LaserShot>
        {
            new LaserShot
            {
                Source = robot1,
                From = new Position(5, 5),
                Direction = Direction.East,
                Priority = 100,
                IsRobotLaser = true,
                Path = new List<Position> { new Position(6, 5), new Position(7, 5) }
            }
        };
        
        var resolver = new LaserResolver();
        resolver.Fire(shots, new List<Player> 
        { 
            new Player { Id = 1, Robot = robot1 },
            new Player { Id = 2, Robot = robot2 }
        });
        
        Assert.Equal(1, robot2.Damage);
    }

    [Fact]
    public void Fire_MultipleShotsSameTarget_AccumulatesDamage()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(7, 5), Direction.North);
        
        var shots = new List<LaserShot>
        {
            new LaserShot
            {
                Source = robot1,
                From = new Position(5, 5),
                Direction = Direction.East,
                Priority = 100,
                IsRobotLaser = true,
                Path = new List<Position> { new Position(7, 5) }
            },
            new LaserShot
            {
                Source = robot1,
                From = new Position(5, 5),
                Direction = Direction.East,
                Priority = 100,
                IsRobotLaser = true,
                Path = new List<Position> { new Position(7, 5) }
            }
        };
        
        var resolver = new LaserResolver();
        resolver.Fire(shots, new List<Player> 
        { 
            new Player { Id = 1, Robot = robot1 },
            new Player { Id = 2, Robot = robot2 }
        });
        
        Assert.Equal(2, robot2.Damage);
    }

    [Fact]
    public void Fire_ShotBlockedByWall_ShouldNotHit()
    {
        var map = new GameMap(10, 10);
        map.SetTile(7, 5, TileFactory.Wall());
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(8, 5), Direction.North);
        
        var shots = new List<LaserShot>
        {
            new LaserShot
            {
                Source = robot1,
                From = new Position(5, 5),
                Direction = Direction.East,
                Priority = 100,
                IsRobotLaser = true,
                Path = new List<Position> { new Position(6, 5), new Position(7, 5) }
            }
        };
        
        var resolver = new LaserResolver();
        resolver.Fire(shots, new List<Player> 
        { 
            new Player { Id = 1, Robot = robot1 },
            new Player { Id = 2, Robot = robot2 }
        });
        
        Assert.Equal(0, robot2.Damage);
    }

    [Fact]
    public void CalculatePath_StraightLine_ShouldReturnAllPositions()
    {
        var map = new GameMap(10, 10);
        var start = new Position(5, 5);
        
        var resolver = new LaserResolver();
        var shots = resolver.PrepareShots(map, new List<Player> 
        { 
            new Player { Id = 1, Robot = new Robot(1, start, Direction.East) }
        });
        
        var path = shots[0].Path;
        
        Assert.Contains(new Position(6, 5), path);
        Assert.Contains(new Position(7, 5), path);
        Assert.Contains(new Position(8, 5), path);
        Assert.Contains(new Position(9, 5), path);
    }

    [Fact]
    public void Priority_ByPosition_ShouldOrderByXY()
    {
        var map = new GameMap(10, 10);
        var robot1 = new Robot(1, new Position(1, 1), Direction.East);
        var robot2 = new Robot(2, new Position(5, 5), Direction.East);
        
        var resolver = new LaserResolver(LaserPriorityMode.ByPosition);
        var shots = resolver.PrepareShots(map, new List<Player> 
        { 
            new Player { Id = 1, Robot = robot1 },
            new Player { Id = 2, Robot = robot2 }
        });
        
        var robot1Shot = shots.First(s => s.Source.Id == 1);
        var robot2Shot = shots.First(s => s.Source.Id == 2);
        
        Assert.True(robot1Shot.Priority < robot2Shot.Priority);
    }

    [Fact]
    public void CalculatePath_PartialWall_ShouldAllowFromOpenSide()
    {
        var map = new GameMap(10, 10);
        var blockedComponent = new BlockedMovementComponent(DirectionSet.FromDirections(Direction.West));
        map.SetTile(7, 5, new Tile(TileType.Floor, new[] { blockedComponent }));
        
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        
        var resolver = new LaserResolver();
        var shots = resolver.PrepareShots(map, new List<Player> { new Player { Id = 1, Robot = robot1 } });
        
        var shot = shots.First(s => s.IsRobotLaser);
        
        Assert.Contains(new Position(7, 5), shot.Path);
    }

    [Fact]
    public void PrepareShots_RobotNextToWall_ShouldNotShootThroughWall()
    {
        var map = new GameMap(10, 10);
        map.SetTile(7, 5, TileFactory.Wall());
        var robot1 = new Robot(1, new Position(5, 5), Direction.East);
        var robot2 = new Robot(2, new Position(8, 5), Direction.North);
        
        var resolver = new LaserResolver();
        var shots = resolver.PrepareShots(map, new List<Player> 
        { 
            new Player { Id = 1, Robot = robot1 },
            new Player { Id = 2, Robot = robot2 }
        });
        
        var shot = shots.First(s => s.Source.Id == 1);
        
        Assert.Contains(new Position(6, 5), shot.Path);
        Assert.DoesNotContain(new Position(7, 5), shot.Path);
        Assert.DoesNotContain(new Position(8, 5), shot.Path);
        
        resolver.Fire(shots, new List<Player> { new Player { Id = 1, Robot = robot1 }, new Player { Id = 2, Robot = robot2 } });
        
        Assert.Equal(0, robot2.Damage);
    }
}
