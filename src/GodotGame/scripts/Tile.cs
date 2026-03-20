namespace AiTest.Godot;

using Godot;

public abstract partial class Tile : Node2D
{
	private const int TileSize = 48;
	private const int MapOffsetY = 120;

	private ColorRect? _baseTile;
	private ColorRect? _borderTile;
	private bool _initialized;
	
	protected Core.Models.Position GridPosition { get; private set; }
	protected Core.Models.TileType TileType { get; private set; }
	protected abstract Color BaseColor { get; }
	protected abstract Color BorderColor { get; }

	public override void _Ready()
	{
		_baseTile = GetNodeOrNull<ColorRect>("BaseTile");
		_borderTile = GetNodeOrNull<ColorRect>("BorderTile");
		
		OnReady();
	}

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
		
		_baseTile ??= GetNodeOrNull<ColorRect>("BaseTile");
		_borderTile ??= GetNodeOrNull<ColorRect>("BorderTile");
		
		if (_baseTile != null)
			_baseTile.Color = BaseColor;
		if (_borderTile != null)
			_borderTile.Color = BorderColor;
		
		_initialized = true;
	}

	public virtual void StartAnimations()
	{
	}

	public virtual void StopAnimations()
	{
	}

	protected void SetBaseColor(Color color)
	{
		if (_baseTile != null)
			_baseTile.Color = color;
	}

	protected void SetBorderColor(Color color)
	{
		if (_borderTile != null)
			_borderTile.Color = color;
	}
}
