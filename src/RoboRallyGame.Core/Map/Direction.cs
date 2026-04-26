namespace RoboRallyGame.Core.Models;

public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public static class DirectionExtensions
{
    public static Direction TurnLeft(this Direction dir) => (Direction)(((int)dir + 3) % 4);
    public static Direction TurnRight(this Direction dir) => (Direction)(((int)dir + 1) % 4);
    public static Direction TurnAround(this Direction dir) => (Direction)(((int)dir + 2) % 4);

    public static (int dx, int dy) ToDelta(this Direction dir) => dir switch
    {
        Direction.North => (0, -1),
        Direction.East => (1, 0),
        Direction.South => (0, 1),
        Direction.West => (-1, 0),
        _ => (0, 0)
    };

    public static char ToSymbol(this Direction dir) => dir switch
    {
        Direction.North => '^',
        Direction.East => '>',
        Direction.South => 'v',
        Direction.West => '<',
        _ => '?'
    };
}
