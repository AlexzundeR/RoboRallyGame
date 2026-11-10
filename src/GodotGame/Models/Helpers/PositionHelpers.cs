using RoboRallyGame.Core.Models;
using RoboRallyGame.Godot.Services;

public static class PositionHelper
{
    public static Vector2 ToTilePosition(this Position pos)
    {
        return new Vector2(pos.X * GameScale.BaseTileSize, pos.Y * GameScale.BaseTileSize);
    }

    public static Vector2 ToTilePositionCentered(this Position pos)
    {
        var width = GameScale.BaseTileSize;
        return new Vector2(pos.X * GameScale.BaseTileSize + width / 2, pos.Y * GameScale.BaseTileSize + width / 2);
    }
}

