namespace AiTest.Godot.Tiles;

public partial class PitTile : Tile
{
    private ColorRect _edgeGlow = null!;
    private bool _isGlowing;

    protected override Color BaseColor => new(0.05f, 0.05f, 0.1f);
    protected override Color BorderColor => new(0.08f, 0.08f, 0.15f);

    protected override void OnReady()
    {
        base.OnReady();
        
        _edgeGlow = new ColorRect
        {
            Name = "EdgeGlow",
            OffsetLeft = 2,
            OffsetTop = 2,
            OffsetRight = 46,
            OffsetBottom = 46,
            Color = new Color(0.8f, 0.1f, 0.1f, 0.3f)
        };
        AddChild(_edgeGlow);
    }

    public override void StartAnimations()
    {
        if (_isGlowing) return;
        _isGlowing = true;
        
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_edgeGlow, "modulate:a", 0.0f, 0.8f);
        tween.TweenProperty(_edgeGlow, "modulate:a", 0.5f, 0.8f);
    }

    public override void StopAnimations()
    {
        _isGlowing = false;
    }
}
