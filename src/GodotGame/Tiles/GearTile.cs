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
            Name = "DirectionLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _directionLabel.AddThemeFontSizeOverride("font_size", 14);
        
        var anchor = new Node2D { Name = "LabelAnchor", Position = new Vector2(24, 24) };
        anchor.AddChild(_directionLabel);
        AddChild(anchor);
    }

    public void Initialize(Core.Models.Position pos, Core.Models.TileType type, bool clockwise)
    {
        base.Initialize(pos, type);
        _clockwise = clockwise;
        
        if (_directionLabel != null)
        {
            _directionLabel.Text = clockwise ? "↻" : "↺";
            _directionLabel.AddThemeColorOverride("font_color", clockwise ? new Color(1f, 0.6f, 0.4f) : new Color(0.6f, 0.8f, 1f));
        }
    }

    public override void StartAnimations()
    {
        if (!IsInsideTree() || _gear == null) return;
        if (_isRotating) return;
        _isRotating = true;
        
        float direction = _clockwise ? 360f : -360f;
        
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_gear, "rotation_degrees", direction / 3f, 3f);
        tween.TweenProperty(_arrow, "rotation_degrees", direction / 3f, 3f);
    }

    public override void StopAnimations()
    {
        _isRotating = false;
    }
}
