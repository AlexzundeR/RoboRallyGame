namespace AiTest.Godot.Tiles;

using Godot;

public partial class GearTile : Tile
{
    private Polygon2D _gear = null!;
    private Polygon2D _arrow = null!;
    private Label _directionLabel = null!;
    private bool _isRotating;
    private bool _clockwise;

    protected override Color BaseColor => new(0.5f, 0.3f, 0.3f);
    protected override Color BorderColor => new(0.6f, 0.4f, 0.4f);

    protected override void OnReady()
    {
        base.OnReady();
        _gear = GetNode<Polygon2D>("Gear");
        _arrow = GetNode<Polygon2D>("Arrow");
        
        _directionLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _directionLabel.AddThemeFontSizeOverride("font_size", 20);
        _directionLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 1f));
        
        AddChild(_directionLabel);
    }

    public void Initialize(Core.Models.Position pos, Core.Models.TileType type, bool clockwise)
    {
        base.Initialize(pos, type);
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
        
        if (_arrow != null)
        {
            tween.TweenProperty(_arrow, "rotation_degrees", rotation / 3f, 3f);
        }
    }

    public override void StopAnimations()
    {
        _isRotating = false;
    }
}
