namespace RoboRallyGame.Godot;

using Godot;
using RoboRallyGame.Godot.Services;

public abstract partial class Tile : Node2D
{
	public int GridX { get; private set; }
	public int GridY { get; private set; }
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
	}

	protected virtual void OnReady()
	{
	}

	public void Initialize(int x, int y, int type)
	{
		GridX = x;
		GridY = y;
		TileTypeIndex = type;
		
		Position = GameScale.GridToWorld(x, y);
		Scale = new Vector2(GameScale.GetMapScale(), GameScale.GetMapScale());
	}

	public virtual void StartAnimations()
	{
	}

	public virtual void StopAnimations()
	{
	}
}
