namespace RoboRallyGame.Godot;

using Godot;
using RoboRallyGame.Godot.Services;

public abstract partial class Tile : Node2D
{	
	protected int GridX { get; private set; }
	protected int GridY { get; private set; }
	protected int TileTypeIndex { get; private set; }
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

	public void Initialize(int x, int y, int type)
	{
		GridX = x;
		GridY = y;
		TileTypeIndex = type;
		Position = new Vector2(x * GameScale.BaseTileSize, y * GameScale.BaseTileSize);
	}

	public virtual void StartAnimations()
	{
	}

	public virtual void StopAnimations()
	{
	}
}
