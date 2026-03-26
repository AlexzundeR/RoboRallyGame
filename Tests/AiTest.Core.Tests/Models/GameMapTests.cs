namespace AiTest.Core.Tests.Models;

using AiTest.Core.Models;
using AiTest.Core.Models.Components;

public class GameMapTests
{
    [Fact]
    public void Create_DefaultMap_ShouldHaveFloorTiles()
    {
        var map = new GameMap(5, 5);
        
        Assert.Equal(5, map.Width);
        Assert.Equal(5, map.Height);
        Assert.Equal(TileType.Floor, map.GetTile(2, 2).Type);
    }

    [Fact]
    public void SetTile_ShouldChangeTileType()
    {
        var map = new GameMap(5, 5);
        map.SetTile(2, 2, TileFactory.Wall());
        
        Assert.Equal(TileType.Wall, map.GetTile(2, 2).Type);
    }

    [Fact]
    public void IsInBounds_InsideBounds_ShouldReturnTrue()
    {
        var map = new GameMap(5, 5);
        
        Assert.True(map.IsInBounds(0, 0));
        Assert.True(map.IsInBounds(4, 4));
        Assert.True(map.IsInBounds(2, 2));
    }

    [Fact]
    public void IsInBounds_OutsideBounds_ShouldReturnFalse()
    {
        var map = new GameMap(5, 5);
        
        Assert.False(map.IsInBounds(-1, 0));
        Assert.False(map.IsInBounds(0, -1));
        Assert.False(map.IsInBounds(5, 0));
        Assert.False(map.IsInBounds(0, 5));
    }

    [Fact]
    public void GetTile_OutsideBounds_ShouldReturnWall()
    {
        var map = new GameMap(5, 5);
        
        Assert.Equal(TileType.Wall, map.GetTile(-1, 0).Type);
        Assert.Equal(TileType.Wall, map.GetTile(5, 5).Type);
    }

    [Fact]
    public void CreateStandardMap_ShouldHaveWalls()
    {
        var map = GameMap.CreateStandardMap();
        
        Assert.Equal(TileType.Wall, map.GetTile(0, 0).Type);
        Assert.Equal(TileType.Wall, map.GetTile(0, 5).Type);
    }

    [Fact]
    public void CreateStandardMap_ShouldHaveStartTiles()
    {
        var map = GameMap.CreateStandardMap();
        
        Assert.Equal(TileType.Start, map.GetTile(2, 2).Type);
        Assert.Equal(TileType.Start, map.GetTile(12, 2).Type);
        Assert.Equal(TileType.Start, map.GetTile(2, 12).Type);
        Assert.Equal(TileType.Start, map.GetTile(12, 12).Type);
    }

    [Fact]
    public void CreateStandardMap_ShouldHaveCheckpoints()
    {
        var map = GameMap.CreateStandardMap();
        
        Assert.Equal(TileType.Checkpoint, map.GetTile(6, 6).Type);
        Assert.Equal(1, map.GetTile(6, 6).CheckpointNumber);
        Assert.Equal(2, map.GetTile(7, 6).CheckpointNumber);
        Assert.Equal(3, map.GetTile(6, 7).CheckpointNumber);
        Assert.Equal(4, map.GetTile(7, 7).CheckpointNumber);
    }

    [Fact]
    public void CreateStandardMap_ShouldHaveConveyors()
    {
        var map = GameMap.CreateStandardMap();
        
        Assert.Equal(TileType.Conveyor, map.GetTile(4, 3).Type);
        Assert.Equal(TileType.Conveyor, map.GetTile(9, 3).Type);
        var conveyor = map.GetTile(3, 5).GetComponent<ConveyorBehaviorComponent>();
        Assert.NotNull(conveyor);
        Assert.True(conveyor!.IsExpress);
    }

    [Fact]
    public void CreateStandardMap_ShouldHavePits()
    {
        var map = GameMap.CreateStandardMap();
        
        Assert.Equal(TileType.Pit, map.GetTile(5, 12).Type);
        Assert.Equal(TileType.Pit, map.GetTile(9, 12).Type);
    }

    [Fact]
    public void CreateStandardMap_ShouldHaveLasers()
    {
        var map = GameMap.CreateStandardMap();
        
        Assert.Equal(TileType.Laser, map.GetTile(2, 5).Type);
    }
}
