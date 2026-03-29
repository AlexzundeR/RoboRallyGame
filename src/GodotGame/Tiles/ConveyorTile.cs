namespace AiTest.Godot.Tiles;

using Godot;
using CoreModels = AiTest.Core.Models;

public partial class ConveyorTile : Tile
{
	private Polygon2D _arrow = null!;
	private Polygon2D _arrow2 = null!;
	private Label _speedLabel = null!;
	private ColorRect _speedLabelBg = null!;
	private bool _isRotating;
	private CoreModels.ConveyorDirection _direction;
	private bool _isExpress;

	protected override Color BaseColor => new(0.25f, 0.4f, 0.5f);
	protected override Color BorderColor => new(0.35f, 0.5f, 0.6f);

	protected override void OnReady()
	{
		base.OnReady();
		_arrow = GetNode<Polygon2D>("Arrow");
		_arrow2 = GetNode<Polygon2D>("Arrow2");
		_speedLabel = GetNode<Label>("SpeedLabel");
		_speedLabelBg = GetNode<ColorRect>("SpeedLabelBg");
	}

	public void Initialize(CoreModels.Position pos, CoreModels.TileType type, CoreModels.ConveyorDirection direction, bool isExpress)
	{
		base.Initialize(pos, type);
		_direction = direction;
		_isExpress = isExpress;
		
		float rotation = direction switch
		{
			CoreModels.ConveyorDirection.North => 0f,
			CoreModels.ConveyorDirection.East => 90f,
			CoreModels.ConveyorDirection.South => 180f,
			CoreModels.ConveyorDirection.West => -90f,
			_ => 0f
		};
		
		if (_arrow != null)
		{
			_arrow.RotationDegrees = rotation;
			_arrow.Visible = true;
		}
		
		if (_arrow2 != null)
		{
			_arrow2.RotationDegrees = rotation;
			_arrow2.Visible = isExpress;
		}
		
		if (_speedLabel != null && _speedLabelBg != null)
		{
			_speedLabel.Text = isExpress ? "2" : "1";
			_speedLabel.AddThemeColorOverride("font_color", isExpress ? new Color(1f, 0.8f, 0.2f) : new Color(0.6f, 0.8f, 0.9f));
			_speedLabelBg.Visible = true;
		}
	}

	public override void StartAnimations()
	{
		if (!IsInsideTree() || _isRotating || _arrow == null)
			return;
		
		_isRotating = true;
		
		float rotation = _direction switch
		{
			CoreModels.ConveyorDirection.North => -360f,
			CoreModels.ConveyorDirection.South => 360f,
			CoreModels.ConveyorDirection.East => -360f,
			CoreModels.ConveyorDirection.West => 360f,
			_ => -360f
		};
		
		var tween = CreateTween();
		tween.SetLoops();
		tween.TweenProperty(_arrow, "rotation_degrees", rotation, 1f);
		
		if (_isExpress && _arrow2 != null)
		{
			tween.TweenProperty(_arrow2, "rotation_degrees", rotation, 1f);
		}
	}

	public override void StopAnimations()
	{
		_isRotating = false;
	}
}
