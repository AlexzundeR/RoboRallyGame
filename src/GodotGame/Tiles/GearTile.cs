namespace RoboRallyGame.Godot.Tiles;

using Godot;

public partial class GearTile : Tile
{
    private Polygon2D _gear = null!;
    private Label _directionLabel = null!;
    private bool _isRotating;
    private bool _clockwise;

    protected override Color BaseColor => new(0.5f, 0.3f, 0.3f);
    protected override Color BorderColor => new(0.6f, 0.4f, 0.4f);

    protected override void OnReady()
    {
        base.OnReady();
        _gear = GetNode<Polygon2D>("Gear");
        _directionLabel = GetNode<Label>("DirectionLabel");
    }

    public void Initialize(int x, int y, int type, bool clockwise)
    {
        Initialize(x, y, type);
        _clockwise = clockwise;
        
        if (_directionLabel != null)
        {
            _directionLabel.Text = clockwise ? "↻" : "↺";
        }
    }

    public override void StartAnimations()
    {
        if (!IsInsideTree() || _isRotating || _gear == null)
            return;
        
        _isRotating = true;
        
        float rotation = _clockwise ? 360f : -360f;
        
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_gear, "rotation_degrees", rotation / 3f, 3f);
    }

    public override void StopAnimations()
    {
        _isRotating = false;
    }
}
