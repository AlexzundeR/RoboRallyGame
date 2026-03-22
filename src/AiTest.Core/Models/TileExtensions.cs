using AiTest.Core.Models.Components;

namespace AiTest.Core.Models;

public static class TileMovementExtensions
{
    public static bool CanEnterFrom(this Tile tile, Direction direction)
    {
        var blocked = tile.GetComponent<BlockedMovementComponent>();
        if (blocked != null)
            return blocked.CanEnterFrom(direction);

        return tile.Type != TileType.Wall;
    }

    public static bool CanExitTo(this Tile tile, Direction direction)
    {
        var blocked = tile.GetComponent<BlockedMovementComponent>();
        if (blocked != null)
            return blocked.CanExitTo(direction);

        return true;
    }

    public static bool IsPassable(this Tile tile)
    {
        if (tile.Type == TileType.Wall)
            return false;

        var blocked = tile.GetComponent<BlockedMovementComponent>();
        if (blocked != null)
            return blocked.BlockedDirections.Count < 4;

        return true;
    }
}
