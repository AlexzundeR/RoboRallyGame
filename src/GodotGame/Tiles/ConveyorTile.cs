namespace AiTest.Godot.Tiles;

using Godot;
using CoreModels = AiTest.Core.Models;

public partial class ConveyorTile : Tile
{
	private Polygon2D _arrow1Vertical = null!;
	private Polygon2D _arrow2Vertical = null!;
	private Polygon2D _arrow1Horizontal = null!;
	private Polygon2D _arrow2Horizontal = null!;

	protected override Color BaseColor => new(0.25f, 0.4f, 0.5f);
	protected override Color BorderColor => new(0.35f, 0.5f, 0.6f);

	protected override void OnReady()
	{
		base.OnReady();
		
		_arrow1Vertical = GetNode<Polygon2D>("ArrowVertical");
		_arrow2Vertical = GetNode<Polygon2D>("Arrow2Vertical");
		_arrow1Horizontal = GetNode<Polygon2D>("ArrowHorizontal");
		_arrow2Horizontal = GetNode<Polygon2D>("Arrow2Horizontal");
	}

	public void Initialize(CoreModels.Position pos, CoreModels.TileType type, CoreModels.ConveyorDirection direction, bool isExpress)
	{
		base.Initialize(pos, type);
		
		bool isVertical = direction == CoreModels.ConveyorDirection.North || direction == CoreModels.ConveyorDirection.South;
		
		_arrow1Vertical.Visible = isVertical;
		_arrow2Vertical.Visible = isVertical && isExpress;
		_arrow1Horizontal.Visible = !isVertical;
		_arrow2Horizontal.Visible = !isVertical && isExpress;
		
		if (isVertical)
		{
			_arrow1Vertical.Position = isExpress ? new Vector2(24, 17) : new Vector2(24, 24);
			_arrow2Vertical.Position = new Vector2(24, 31);
		}
		else
		{
			_arrow1Horizontal.Position = isExpress ? new Vector2(17, 24) : new Vector2(24, 24);
			_arrow2Horizontal.Position = new Vector2(31, 24);
		}
	}

	public override void StartAnimations() { }
	public override void StopAnimations() { }
}
