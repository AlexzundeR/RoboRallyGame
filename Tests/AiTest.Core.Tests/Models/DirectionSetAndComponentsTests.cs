namespace AiTest.Core.Tests.Models;

using AiTest.Core.Models;
using AiTest.Core.Models.Components;

public class DirectionSetTests
{
    [Fact]
    public void Empty_ShouldHaveNoDirections()
    {
        var set = DirectionSet.Empty;
        
        Assert.Equal(0, set.Count);
        Assert.False(set.Contains(Direction.North));
        Assert.False(set.Contains(Direction.East));
        Assert.False(set.Contains(Direction.South));
        Assert.False(set.Contains(Direction.West));
    }
    
    [Fact]
    public void All_ShouldHaveAllDirections()
    {
        var set = DirectionSet.All;
        
        Assert.Equal(4, set.Count);
        Assert.True(set.Contains(Direction.North));
        Assert.True(set.Contains(Direction.East));
        Assert.True(set.Contains(Direction.South));
        Assert.True(set.Contains(Direction.West));
    }
    
    [Fact]
    public void FromDirections_ShouldCreateCorrectSet()
    {
        var set = DirectionSet.FromDirections(Direction.North, Direction.South);
        
        Assert.Equal(2, set.Count);
        Assert.True(set.Contains(Direction.North));
        Assert.True(set.Contains(Direction.South));
        Assert.False(set.Contains(Direction.East));
        Assert.False(set.Contains(Direction.West));
    }
    
    [Fact]
    public void Add_ShouldAddDirection()
    {
        var set = DirectionSet.North.Add(Direction.East);
        
        Assert.Equal(2, set.Count);
        Assert.True(set.Contains(Direction.North));
        Assert.True(set.Contains(Direction.East));
    }
    
    [Fact]
    public void Remove_ShouldRemoveDirection()
    {
        var set = DirectionSet.All.Remove(Direction.North);
        
        Assert.Equal(3, set.Count);
        Assert.False(set.Contains(Direction.North));
        Assert.True(set.Contains(Direction.East));
    }
    
    [Fact]
    public void Union_ShouldCombineSets()
    {
        var set1 = DirectionSet.FromDirections(Direction.North, Direction.East);
        var set2 = DirectionSet.FromDirections(Direction.South, Direction.West);
        
        var result = set1.Union(set2);
        
        Assert.True(result.Contains(Direction.North));
        Assert.True(result.Contains(Direction.East));
        Assert.True(result.Contains(Direction.South));
        Assert.True(result.Contains(Direction.West));
    }
    
    [Fact]
    public void Intersect_ShouldReturnCommon()
    {
        var set1 = DirectionSet.FromDirections(Direction.North, Direction.East, Direction.South);
        var set2 = DirectionSet.FromDirections(Direction.East, Direction.South, Direction.West);
        
        var result = set1.Intersect(set2);
        
        Assert.Equal(2, result.Count);
        Assert.True(result.Contains(Direction.East));
        Assert.True(result.Contains(Direction.South));
    }
    
    [Fact]
    public void Except_ShouldReturnDifference()
    {
        var set1 = DirectionSet.All;
        var set2 = DirectionSet.FromDirections(Direction.North, Direction.South);
        
        var result = set1.Except(set2);
        
        Assert.Equal(2, result.Count);
        Assert.True(result.Contains(Direction.East));
        Assert.True(result.Contains(Direction.West));
    }
    
    [Fact]
    public void First_ShouldReturnFirstDirection()
    {
        var set = DirectionSet.FromDirections(Direction.West, Direction.North);
        
        var first = set.First();
        
        Assert.Equal(Direction.North, first);
    }
    
    [Fact]
    public void First_EmptySet_ShouldReturnNull()
    {
        var first = DirectionSet.Empty.First();
        
        Assert.Null(first);
    }
    
    [Fact]
    public void AllDirections_ShouldEnumerateAllDirections()
    {
        var set = DirectionSet.FromDirections(Direction.North, Direction.West);
        
        var directions = set.AllDirections().ToList();
        
        Assert.Equal(2, directions.Count);
        Assert.Contains(Direction.North, directions);
        Assert.Contains(Direction.West, directions);
    }
    
    [Fact]
    public void Operators_ShouldWorkCorrectly()
    {
        var set1 = DirectionSet.North;
        var set2 = DirectionSet.East;
        
        Assert.Equal(2, (set1 + set2).Count);
        Assert.Equal(0, (set1 & set2).Count);
    }
}

public class BlockedMovementComponentTests
{
    [Fact]
    public void AllBlocked_ShouldBlockAllDirections()
    {
        var component = BlockedMovementComponent.AllBlocked();
        
        Assert.Equal(DirectionSet.All, component.BlockedDirections);
        Assert.False(component.CanEnterFrom(Direction.North));
        Assert.False(component.CanEnterFrom(Direction.East));
        Assert.False(component.CanEnterFrom(Direction.South));
        Assert.False(component.CanEnterFrom(Direction.West));
    }
    
    [Fact]
    public void NoneBlocked_ShouldAllowAll()
    {
        var component = BlockedMovementComponent.NoneBlocked();
        
        Assert.Equal(DirectionSet.Empty, component.BlockedDirections);
        Assert.True(component.CanEnterFrom(Direction.North));
        Assert.True(component.CanEnterFrom(Direction.East));
        Assert.True(component.CanEnterFrom(Direction.South));
        Assert.True(component.CanEnterFrom(Direction.West));
    }
    
    [Fact]
    public void BlockTwoDirections_ShouldAllowOthers()
    {
        var component = BlockedMovementComponent.Block(Direction.North, Direction.South);
        
        Assert.False(component.CanEnterFrom(Direction.North));
        Assert.False(component.CanEnterFrom(Direction.South));
        Assert.True(component.CanEnterFrom(Direction.East));
        Assert.True(component.CanEnterFrom(Direction.West));
    }
    
    [Fact]
    public void AllowedDirections_ShouldBeInverseOfBlocked()
    {
        var blocked = DirectionSet.FromDirections(Direction.North, Direction.South);
        var component = new BlockedMovementComponent(blocked);
        
        Assert.True(component.AllowedDirections.Contains(Direction.East));
        Assert.True(component.AllowedDirections.Contains(Direction.West));
        Assert.False(component.AllowedDirections.Contains(Direction.North));
        Assert.False(component.AllowedDirections.Contains(Direction.South));
    }
}

public class TileWithComponentsTests
{
    [Fact]
    public void Tile_WithBlockedMovementComponent_ShouldAllowOnlyUnblockedDirections()
    {
        var tile = TileFactory.Barricade(Direction.East, Direction.West);
        
        Assert.True(tile.CanEnterFrom(Direction.East));
        Assert.True(tile.CanEnterFrom(Direction.West));
        Assert.False(tile.CanEnterFrom(Direction.North));
        Assert.False(tile.CanEnterFrom(Direction.South));
    }
    
    [Fact]
    public void Tile_WithComponent_ShouldGetComponent()
    {
        var tile = TileFactory.Barricade(Direction.East);
        var blocked = tile.GetComponent<BlockedMovementComponent>();
        
        Assert.NotNull(blocked);
        Assert.True(blocked!.BlockedDirections.Contains(Direction.North));
        Assert.True(blocked.BlockedDirections.Contains(Direction.South));
        Assert.True(blocked.BlockedDirections.Contains(Direction.West));
    }
    
    [Fact]
    public void Tile_WithoutComponent_ShouldReturnNull()
    {
        var tile = TileFactory.Floor();
        var blocked = tile.GetComponent<BlockedMovementComponent>();
        
        Assert.Null(blocked);
    }
    
    [Fact]
    public void TileFactory_TunnelNS_ShouldBlockEastAndWest()
    {
        var tile = TileFactory.TunnelNS();
        
        Assert.Equal(TileType.Floor, tile.Type);
        Assert.True(tile.CanEnterFrom(Direction.North));
        Assert.True(tile.CanEnterFrom(Direction.South));
        Assert.False(tile.CanEnterFrom(Direction.East));
        Assert.False(tile.CanEnterFrom(Direction.West));
    }
    
    [Fact]
    public void TileFactory_TunnelEW_ShouldBlockNorthAndSouth()
    {
        var tile = TileFactory.TunnelEW();
        
        Assert.Equal(TileType.Floor, tile.Type);
        Assert.True(tile.CanEnterFrom(Direction.East));
        Assert.True(tile.CanEnterFrom(Direction.West));
        Assert.False(tile.CanEnterFrom(Direction.North));
        Assert.False(tile.CanEnterFrom(Direction.South));
    }
    
    [Fact]
    public void TileFactory_BarricadeSouth_ShouldBlockOnlySouth()
    {
        var tile = TileFactory.Barricade(Direction.South);
        
        Assert.Equal(TileType.Floor, tile.Type);
        Assert.True(tile.CanEnterFrom(Direction.South));
        Assert.False(tile.CanEnterFrom(Direction.North));
        Assert.False(tile.CanEnterFrom(Direction.East));
        Assert.False(tile.CanEnterFrom(Direction.West));
    }
    
    [Fact]
    public void Floor_CanExitTo_ShouldAlwaysReturnTrue()
    {
        var tile = TileFactory.Floor();
        
        Assert.True(tile.CanExitTo(Direction.North));
        Assert.True(tile.CanExitTo(Direction.South));
        Assert.True(tile.CanExitTo(Direction.East));
        Assert.True(tile.CanExitTo(Direction.West));
    }
    
    [Fact]
    public void WallWithBlocked_ShouldRespectExitBlockage()
    {
        var tile = TileFactory.Barricade(Direction.East);
        
        Assert.True(tile.CanExitTo(Direction.East));
        Assert.False(tile.CanExitTo(Direction.West));
    }
    
    [Fact]
    public void Wall_WithoutComponent_ShouldNotBeEnterable()
    {
        var tile = TileFactory.Wall();
        
        Assert.False(tile.CanEnterFrom(Direction.North));
        Assert.False(tile.CanEnterFrom(Direction.East));
        Assert.False(tile.CanEnterFrom(Direction.South));
        Assert.False(tile.CanEnterFrom(Direction.West));
    }
    
    [Fact]
    public void Floor_ShouldBeEnterable()
    {
        var tile = TileFactory.Floor();
        
        Assert.True(tile.CanEnterFrom(Direction.North));
        Assert.True(tile.CanEnterFrom(Direction.East));
        Assert.True(tile.CanEnterFrom(Direction.South));
        Assert.True(tile.CanEnterFrom(Direction.West));
    }
}
