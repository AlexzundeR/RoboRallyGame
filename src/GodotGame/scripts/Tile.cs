namespace AiTest.Godot;

using Godot;
using AiTest.Godot.Services;

public abstract partial class Tile : Node2D
{	
	protected Core.Models.Position GridPosition { get; private set; }
	protected Core.Models.TileType TileType { get; private set; }
	protected abstract Color BaseColor { get; }
	protected abstract Color BorderColor { get; }

	public override void _Ready()
	{
		OnReady();
		
		var baseTile = GetNode<ColorRect>("BaseTile");
		var borderTile = GetNode<ColorRect>("BorderTile");
		
		baseTile.Color = BaseColor;
		borderTile.Color = BorderColor;
		
		var tileSize = GameScale.Instance?.TileSize ?? GameScale.BaseTileSize;
		baseTile.Size = new Vector2(tileSize, tileSize);
		borderTile.Size = new Vector2(tileSize, tileSize);
	}

	protected virtual void OnReady()
	{
	}

	public void Initialize(Core.Models.Position position, Core.Models.TileType type)
	{
		GridPosition = position;
		TileType = type;
		Position = new Vector2(position.X * GameScale.BaseTileSize, position.Y * GameScale.BaseTileSize);
	}

	public virtual void StartAnimations()
	{
	}

	public virtual void StopAnimations()
	{
	}
}
