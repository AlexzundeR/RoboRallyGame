namespace AiTest.Godot;

using Godot;

public abstract partial class Tile : Node2D
{
	private const int TileSize = 48;
	private const int MapOffsetY = 120;
	
	protected Core.Models.Position GridPosition { get; private set; }
	protected Core.Models.TileType TileType { get; private set; }
	protected abstract Color BaseColor { get; }
	protected abstract Color BorderColor { get; }

	protected virtual void OnReady()
	{
	}

	public void Initialize(Core.Models.Position position, Core.Models.TileType type)
	{
		GridPosition = position;
		TileType = type;
		
		Position = new Vector2(
			position.X * TileSize,
			position.Y * TileSize + MapOffsetY
		);
		
		if (!IsInsideTree()) return;
		
		var baseTile = GetNode<ColorRect>("BaseTile");
		var borderTile = GetNode<ColorRect>("BorderTile");
		
		baseTile.Color = BaseColor;
		borderTile.Color = BorderColor;
	}

	public virtual void StartAnimations()
	{
	}

	public virtual void StopAnimations()
	{
	}
}
