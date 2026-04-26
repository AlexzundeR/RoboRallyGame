namespace RoboRallyGame.Core.Tests.Models;

using RoboRallyGame.Core.Models;

public class DirectionTests
{
    [Theory]
    [InlineData(Direction.North, Direction.West)]
    [InlineData(Direction.East, Direction.North)]
    [InlineData(Direction.South, Direction.East)]
    [InlineData(Direction.West, Direction.South)]
    public void TurnLeft_ShouldReturnCorrectDirection(Direction start, Direction expected)
    {
        Assert.Equal(expected, start.TurnLeft());
    }

    [Theory]
    [InlineData(Direction.North, Direction.East)]
    [InlineData(Direction.East, Direction.South)]
    [InlineData(Direction.South, Direction.West)]
    [InlineData(Direction.West, Direction.North)]
    public void TurnRight_ShouldReturnCorrectDirection(Direction start, Direction expected)
    {
        Assert.Equal(expected, start.TurnRight());
    }

    [Theory]
    [InlineData(Direction.North, Direction.South)]
    [InlineData(Direction.East, Direction.West)]
    [InlineData(Direction.South, Direction.North)]
    [InlineData(Direction.West, Direction.East)]
    public void TurnAround_ShouldReturnOppositeDirection(Direction start, Direction expected)
    {
        Assert.Equal(expected, start.TurnAround());
    }

    [Fact]
    public void ToDelta_North_ShouldReturnCorrectDelta()
    {
        var (dx, dy) = Direction.North.ToDelta();
        Assert.Equal(0, dx);
        Assert.Equal(-1, dy);
    }

    [Fact]
    public void ToDelta_East_ShouldReturnCorrectDelta()
    {
        var (dx, dy) = Direction.East.ToDelta();
        Assert.Equal(1, dx);
        Assert.Equal(0, dy);
    }

    [Fact]
    public void ToSymbol_ShouldReturnCorrectChar()
    {
        Assert.Equal('^', Direction.North.ToSymbol());
        Assert.Equal('>', Direction.East.ToSymbol());
        Assert.Equal('v', Direction.South.ToSymbol());
        Assert.Equal('<', Direction.West.ToSymbol());
    }
}
