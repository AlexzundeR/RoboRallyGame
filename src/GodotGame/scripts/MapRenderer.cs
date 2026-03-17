using Godot;
using System.Collections.Generic;
using AiTest.Core.Models;

namespace AiTest.Godot;

public partial class MapRenderer : Node2D
{
    private readonly Dictionary<Position, ColorRect> _tileVisuals = new();
    private const int TileSize = 48;
    private const int MapOffsetY = 120;

    public void RenderMap(GameMap map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var pos = new Position(x, y);
                var tile = map.GetTile(pos);
                var color = GetTileColor(tile);
                CreateTileVisual(x, y, color);

                if (tile.Type == TileType.Checkpoint)
                {
                    CreateCheckpointMarker(x, y, tile.CheckpointNumber ?? 1);
                }
            }
        }
    }

    private Color GetTileColor(Tile tile)
    {
        return tile.Type switch
        {
            TileType.Floor => new Color(0.2f, 0.2f, 0.25f),
            TileType.Wall => new Color(0.4f, 0.4f, 0.45f),
            TileType.Start => new Color(0.15f, 0.35f, 0.15f),
            TileType.Pit => new Color(0.05f, 0.05f, 0.1f),
            TileType.Checkpoint => new Color(0.3f, 0.25f, 0.1f),
            TileType.ConveyorCW or TileType.ConveyorCCW => new Color(0.25f, 0.4f, 0.5f),
            TileType.ExpressConveyorCW or TileType.ExpressConveyorCCW => new Color(0.15f, 0.5f, 0.7f),
            TileType.GearCW or TileType.GearCCW => new Color(0.5f, 0.3f, 0.3f),
            _ => new Color(0.2f, 0.2f, 0.25f)
        };
    }

	private void CreateTileVisual(int x, int y, Color color)
	{
		var tile = new ColorRect
		{
			Position = new Vector2(x * TileSize, y * TileSize + MapOffsetY),
			Size = new Vector2(TileSize, TileSize),
			Color = color
		};

		var border = new ColorRect
		{
			Position = new Vector2(x * TileSize + 1, y * TileSize + MapOffsetY + 1),
			Size = new Vector2(TileSize - 2, TileSize - 2),
			Color = new Color(color.R + 0.1f, color.G + 0.1f, color.B + 0.1f)
		};

		AddChild(tile);
		AddChild(border);
	}

	private void CreateCheckpointMarker(int x, int y, int number)
	{
		var label = new Label
		{
			Text = "★" + number,
			Position = new Vector2(x * TileSize + 16, y * TileSize + MapOffsetY + 20)
		};
        label.AddThemeFontSizeOverride("font_size", 24);
        label.AddThemeColorOverride("font_color", Colors.Gold);
        AddChild(label);
    }
}
