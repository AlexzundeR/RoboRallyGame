namespace RoboRallyGame.Core.Models;

public readonly struct DirectionSet
{
    private readonly byte _mask;

    private DirectionSet(byte mask) => _mask = mask;

    public static DirectionSet Empty => new(0);
    public static DirectionSet All => new(0b1111);
    public static DirectionSet North => new(1 << (int)Direction.North);
    public static DirectionSet East => new(1 << (int)Direction.East);
    public static DirectionSet South => new(1 << (int)Direction.South);
    public static DirectionSet West => new(1 << (int)Direction.West);

    public static DirectionSet FromDirections(params Direction[] directions)
    {
        byte mask = 0;
        foreach (var dir in directions)
            mask |= (byte)(1 << (int)dir);
        return new DirectionSet(mask);
    }

    public bool Contains(Direction direction) => (_mask & (1 << (int)direction)) != 0;

    public int Count
    {
        get
        {
            int count = 0;
            byte temp = _mask;
            while (temp != 0)
            {
                count += temp & 1;
                temp >>= 1;
            }
            return count;
        }
    }

    public DirectionSet Union(DirectionSet other) => new((byte)(_mask | other._mask));

    public DirectionSet Intersect(DirectionSet other) => new((byte)(_mask & other._mask));

    public DirectionSet Except(DirectionSet other) => new((byte)(_mask & ~other._mask));

    public DirectionSet Add(Direction direction) => new((byte)(_mask | (1 << (int)direction)));

    public DirectionSet Remove(Direction direction) => new((byte)(_mask & ~(1 << (int)direction)));

    public Direction? First()
    {
        for (int i = 0; i < 4; i++)
            if ((_mask & (1 << i)) != 0)
                return (Direction)i;
        return null;
    }

    public IEnumerable<Direction> AllDirections()
    {
        for (int i = 0; i < 4; i++)
            if ((_mask & (1 << i)) != 0)
                yield return (Direction)i;
    }

    public bool IsSubsetOf(DirectionSet other) => (_mask & ~other._mask) == 0;

    public bool IsSupersetOf(DirectionSet other) => (other._mask & ~_mask) == 0;

    public static DirectionSet operator +(DirectionSet a, DirectionSet b) => a.Union(b);
    public static DirectionSet operator &(DirectionSet a, DirectionSet b) => a.Intersect(b);
    public static DirectionSet operator -(DirectionSet a, DirectionSet b) => a.Except(b);

    public override bool Equals(object? obj) => obj is DirectionSet other && _mask == other._mask;
    public override int GetHashCode() => _mask.GetHashCode();
    public override string ToString() => $"[{string.Join(",", AllDirections())}]";
}
