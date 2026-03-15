namespace AiTest.Core.Tests.Models;

using AiTest.Core.Models;

public class PositionTests
{
    [Fact]
    public void Move_North_ShouldDecreaseY()
    {
        var pos = new Position(5, 5);
        var newPos = pos.Move(Direction.North);
        
        Assert.Equal(5, newPos.X);
        Assert.Equal(4, newPos.Y);
    }

    [Fact]
    public void Move_East_ShouldIncreaseX()
    {
        var pos = new Position(5, 5);
        var newPos = pos.Move(Direction.East);
        
        Assert.Equal(6, newPos.X);
        Assert.Equal(5, newPos.Y);
    }

    [Fact]
    public void Move_South_ShouldIncreaseY()
    {
        var pos = new Position(5, 5);
        var newPos = pos.Move(Direction.South);
        
        Assert.Equal(5, newPos.X);
        Assert.Equal(6, newPos.Y);
    }

    [Fact]
    public void Move_West_ShouldDecreaseX()
    {
        var pos = new Position(5, 5);
        var newPos = pos.Move(Direction.West);
        
        Assert.Equal(4, newPos.X);
        Assert.Equal(5, newPos.Y);
    }

    [Fact]
    public void IsValid_WithinBounds_ShouldReturnTrue()
    {
        var pos = new Position(5, 5);
        
        Assert.True(pos.IsValid(10, 10));
    }

    [Fact]
    public void IsValid_OutsideBounds_ShouldReturnFalse()
    {
        var pos = new Position(10, 5);
        
        Assert.False(pos.IsValid(10, 10));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(9, 9)]
    public void ToString_ShouldReturnExpectedFormat(int x, int y)
    {
        var pos = new Position(x, y);
        
        Assert.Equal($"({x}, {y})", pos.ToString());
    }
}
