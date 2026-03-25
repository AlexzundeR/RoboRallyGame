namespace AiTest.Core.Tests.Models;

using AiTest.Core.Models;

public class TileTests
{
    [Fact]
    public void Floor_ShouldHaveFloorType()
    {
        var tile = Tile.Floor;
        Assert.Equal(TileType.Floor, tile.Type);
    }

    [Fact]
    public void Wall_ShouldHaveWallType()
    {
        var tile = Tile.Wall;
        Assert.Equal(TileType.Wall, tile.Type);
    }

    [Fact]
    public void Pit_ShouldHavePitType()
    {
        var tile = Tile.Pit;
        Assert.Equal(TileType.Pit, tile.Type);
    }

    [Fact]
    public void Conveyor_ShouldHaveCorrectDirection()
    {
        var tile = Tile.Conveyor(ConveyorDirection.North);
        Assert.Equal(TileType.Conveyor, tile.Type);
        Assert.Equal(ConveyorDirection.North, tile.ConveyorDir);
    }

    [Fact]
    public void Conveyor_WithExpress_ShouldHaveIsExpress()
    {
        var tile = Tile.Conveyor(ConveyorDirection.East, true);
        Assert.Equal(TileType.Conveyor, tile.Type);
        Assert.True(tile.IsExpress);
    }

    [Fact]
    public void Gear_Clockwise_ShouldHaveCorrectProperty()
    {
        var tile = Tile.Gear(true);
        Assert.Equal(TileType.Gear, tile.Type);
        Assert.True(tile.IsClockwise);
    }

    [Fact]
    public void Gear_CounterClockwise_ShouldHaveCorrectProperty()
    {
        var tile = Tile.Gear(false);
        Assert.Equal(TileType.Gear, tile.Type);
        Assert.False(tile.IsClockwise);
    }

    [Fact]
    public void Laser_ShouldHaveCorrectDirection()
    {
        var tile = Tile.Laser(Direction.East);
        Assert.Equal(TileType.Laser, tile.Type);
        Assert.Equal(Direction.East, tile.LaserDirection);
    }

    [Fact]
    public void Checkpoint_ShouldHaveCorrectNumber()
    {
        var tile = Tile.Checkpoint(3);
        Assert.Equal(TileType.Checkpoint, tile.Type);
        Assert.Equal(3, tile.CheckpointNumber);
    }

    [Fact]
    public void Start_ShouldHaveStartType()
    {
        var tile = Tile.Start;
        Assert.Equal(TileType.Start, tile.Type);
    }

    [Theory]
    [InlineData(TileType.Floor, '·')]
    [InlineData(TileType.Wall, '█')]
    [InlineData(TileType.Pit, ' ')]
    [InlineData(TileType.Start, 'S')]
    public void ToChar_ShouldReturnExpectedChar(TileType type, char expected)
    {
        var tile = new Tile(type);
        Assert.Equal(expected, tile.ToChar());
    }
}
