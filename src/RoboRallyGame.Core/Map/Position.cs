namespace RoboRallyGame.Core.Models;

public readonly record struct Position(int X, int Y)
{
    public Position Move(Direction dir) => this with
    {
        X = X + dir.ToDelta().dx,
        Y = Y + dir.ToDelta().dy
    };

    public static Position operator +(Position a, Position b) => new(a.X + b.X, a.Y + b.Y);
    public static Position operator -(Position a, Position b) => new(a.X - b.X, a.Y - b.Y);

    public bool IsValid(int width, int height) => X >= 0 && X < width && Y >= 0 && Y < height;

    public override string ToString() => $"({X}, {Y})";
}
