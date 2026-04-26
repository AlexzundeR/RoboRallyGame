namespace RoboRallyGame.Godot.Tiles;

using Godot;

public partial class LaserTile : Tile
{
    private Polygon2D _emitter = null!;
    private bool _isPulsing;

    protected override Color BaseColor => new(0.2f, 0.2f, 0.25f);
    protected override Color BorderColor => new(0.3f, 0.3f, 0.35f);

    protected override void OnReady()
    {
        base.OnReady();
        _emitter = GetNode<Polygon2D>("LaserEmitter");
    }

    public override void StartAnimations()
    {
        if (!IsInsideTree() || _emitter == null) return;
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
