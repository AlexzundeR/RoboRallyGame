namespace RoboRallyGame.Core.Models.Components;

public sealed class BlockedMovementComponent : ITileComponent
{
    public DirectionSet BlockedDirections { get; }

    public DirectionSet AllowedDirections => DirectionSet.All.Except(BlockedDirections);

    public BlockedMovementComponent(DirectionSet blockedDirections)
    {
        BlockedDirections = blockedDirections;
    }

    public static BlockedMovementComponent AllBlocked() => new(DirectionSet.All);

    public static BlockedMovementComponent NoneBlocked() => new(DirectionSet.Empty);

    public static BlockedMovementComponent Block(Direction d1) => new(DirectionSet.FromDirections(d1));

    public static BlockedMovementComponent Block(Direction d1, Direction d2) 
        => new(DirectionSet.FromDirections(d1, d2));

    public static BlockedMovementComponent Block(Direction d1, Direction d2, Direction d3) 
        => new(DirectionSet.FromDirections(d1, d2, d3));

    public static BlockedMovementComponent Block(Direction d1, Direction d2, Direction d3, Direction d4) 
        => new(DirectionSet.FromDirections(d1, d2, d3, d4));

    public bool CanEnterFrom(Direction direction) => !BlockedDirections.Contains(direction);
    public bool CanExitTo(Direction direction) => !BlockedDirections.Contains(direction);
}
