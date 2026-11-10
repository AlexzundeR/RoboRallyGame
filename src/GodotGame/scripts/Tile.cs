namespace RoboRallyGame.Godot;

using Godot;

using RoboRallyGame.Core.Models;
using RoboRallyGame.Godot.Services;

public abstract partial class Tile : Node2D
{
	private int _gridX;
	private int _gridY;
	private int _tileTypeIndex;
	protected abstract Color BaseColor { get; }
	protected abstract Color BorderColor { get; }

	public override void _Ready()
	{
		OnReady();
		
		var baseTile = GetNode<ColorRect>("BaseTile");
		var borderTile = GetNode<ColorRect>("BorderTile");
		
		baseTile.Color = BaseColor;
		borderTile.Color = BorderColor;

		var tileSize = GameScale.BaseTileSize;
		baseTile.Size = new Vector2(tileSize, tileSize);
		borderTile.Size = new Vector2(tileSize, tileSize);
	}

	public override void _Process(double delta)
	{
		if (!IsInsideTree()) return;
		
		Position = new Position(_gridX, _gridY).ToTilePosition();
	}

	protected virtual void OnReady()
	{
	}

	public void Initialize(int x, int y, int type)
	{
		_gridX = x;
		_gridY = y;
		_tileTypeIndex = type;
		
		Position = new Position(x, y).ToTilePosition();
	}

	public virtual void StartAnimations()
	{
	}

	public virtual void StopAnimations()
	{
	}
}
