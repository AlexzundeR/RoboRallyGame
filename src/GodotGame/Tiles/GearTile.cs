namespace AiTest.Godot.Tiles;

using Godot;

public partial class GearTile : Tile
{
    private Polygon2D _gear = null!;
    private bool _isRotating;
    private bool _clockwise;

    protected override Color BaseColor => new(0.5f, 0.3f, 0.3f);
    protected override Color BorderColor => new(0.6f, 0.4f, 0.4f);

    protected override void OnReady()
    {
        base.OnReady();
        _gear = GetNode<Polygon2D>("Gear");
    }

    public void Initialize(Core.Models.Position pos, Core.Models.TileType type)
    {
        base.Initialize(pos, type);
        _clockwise = type == Core.Models.TileType.GearCW;
    }

    public override void StartAnimations()
    {
        if (_isRotating) return;
        _isRotating = true;
        
        float direction = _clockwise ? 360f : -360f;
        
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_gear, "rotation_degrees", direction / 3f, 3f);
    }

    public override void StopAnimations()
    {
        _isRotating = false;
    }
}
