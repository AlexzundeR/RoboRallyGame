namespace AiTest.Godot.Tiles;

public partial class LaserTile : Tile
{
    private Polygon2D _emitter = null!;
    private bool _isPulsing;

    protected override Color BaseColor => new(0.2f, 0.2f, 0.25f);
    protected override Color BorderColor => new(0.3f, 0.3f, 0.35f);

    protected override void OnReady()
    {
        base.OnReady();
        
        _emitter = new Polygon2D
        {
            Name = "Emitter",
            Position = new Vector2(24, 24),
            Polygon = new[] { new Vector2(-6, -6), new Vector2(6, -6), new Vector2(6, 6), new Vector2(-6, 6) },
            Color = new Color(1.0f, 0.2f, 0.2f, 0.8f)
        };
        AddChild(_emitter);
    }

    public override void StartAnimations()
    {
        if (_isPulsing) return;
        _isPulsing = true;
        
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_emitter, "modulate:a", 1.0f, 0.3f);
        tween.TweenProperty(_emitter, "modulate:a", 0.3f, 1.5f);
    }

    public override void StopAnimations()
    {
        _isPulsing = false;
    }
}
