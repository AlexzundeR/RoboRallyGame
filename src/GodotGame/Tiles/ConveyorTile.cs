namespace AiTest.Godot.Tiles;

using Godot;
using GodotModels = Models;

public partial class ConveyorTile : Tile
{
	private Node2D _arrowContainer = null!;
	private Node2D _singleArrow = null!;
	private Node2D _doubleArrow = null!;

	protected override Color BaseColor => new(0.25f, 0.4f, 0.5f);
	protected override Color BorderColor => new(0.35f, 0.5f, 0.6f);

	protected override void OnReady()
	{
		base.OnReady();
		
		_arrowContainer = GetNode<Node2D>("ArrowContainer");
		_singleArrow = GetNode<Node2D>("ArrowContainer/SingleArrow");
		_doubleArrow = GetNode<Node2D>("ArrowContainer/DoubleArrow");
	}

	public void Initialize(int x, int y, int type, GodotModels.ConveyorDirection direction, bool isExpress)
	{
        Initialize(x, y, type);
		
		float rotation = direction switch
		{
			GodotModels.ConveyorDirection.North => 0f,
			GodotModels.ConveyorDirection.East => 90f,
			GodotModels.ConveyorDirection.South => 180f,
			GodotModels.ConveyorDirection.West => -90f,
			_ => 0f
		};
		
		_arrowContainer.RotationDegrees = rotation;
		
		_singleArrow.Visible = !isExpress;
		_doubleArrow.Visible = isExpress;
	}

	public override void StartAnimations() { }
	public override void StopAnimations() { }
}
