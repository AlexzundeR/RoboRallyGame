namespace RoboRallyGame.Core.Tests.Models;

using RoboRallyGame.Core.Models;
using RoboRallyGame.Core.Models.Components;

public class TileTests
{
    [Fact]
    public void Floor_ShouldHaveFloorType()
    {
        var tile = TileFactory.Floor();
        Assert.Equal(TileType.Floor, tile.Type);
    }

    [Fact]
    public void Wall_ShouldHaveWallType()
    {
        var tile = TileFactory.Wall();
        Assert.Equal(TileType.Wall, tile.Type);
    }

    [Fact]
    public void Pit_ShouldHavePitType()
    {
        var tile = TileFactory.Pit();
        Assert.Equal(TileType.Pit, tile.Type);
    }

    [Fact]
    public void Conveyor_ShouldHaveCorrectDirection()
    {
        var tile = TileFactory.Conveyor(ConveyorDirection.North);
        Assert.Equal(TileType.Conveyor, tile.Type);
        var conveyor = tile.GetComponent<ConveyorBehaviorComponent>();
        Assert.NotNull(conveyor);
        Assert.Equal(ConveyorDirection.North, conveyor!.Direction);
    }

    [Fact]
    public void Conveyor_WithExpress_ShouldHaveIsExpress()
    {
        var tile = TileFactory.Conveyor(ConveyorDirection.East, true);
        Assert.Equal(TileType.Conveyor, tile.Type);
        var conveyor = tile.GetComponent<ConveyorBehaviorComponent>();
        Assert.NotNull(conveyor);
        Assert.True(conveyor!.IsExpress);
    }

    [Fact]
    public void Gear_Clockwise_ShouldHaveCorrectProperty()
    {
        var tile = TileFactory.Gear(true);
        Assert.Equal(TileType.Gear, tile.Type);
        var gear = tile.GetComponent<GearBehaviorComponent>();
        Assert.NotNull(gear);
        Assert.True(gear!.IsClockwise);
    }

    [Fact]
    public void Gear_CounterClockwise_ShouldHaveCorrectProperty()
    {
        var tile = TileFactory.Gear(false);
        Assert.Equal(TileType.Gear, tile.Type);
        var gear = tile.GetComponent<GearBehaviorComponent>();
        Assert.NotNull(gear);
        Assert.False(gear!.IsClockwise);
    }

    [Fact]
    public void Laser_ShouldHaveCorrectDirection()
    {
        var tile = TileFactory.Laser(Direction.East);
        Assert.Equal(TileType.Laser, tile.Type);
        Assert.Equal(Direction.East, tile.LaserDirection);
    }

    [Fact]
    public void Checkpoint_ShouldHaveCorrectNumber()
    {
        var tile = TileFactory.Checkpoint(3);
        Assert.Equal(TileType.Checkpoint, tile.Type);
        Assert.Equal(3, tile.CheckpointNumber);
    }

    [Fact]
    public void Start_ShouldHaveStartType()
    {
        var tile = TileFactory.Start();
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
