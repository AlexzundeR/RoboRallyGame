namespace AiTest.Godot.Tiles;

using Godot;

public partial class ConveyorTile : Tile
{
    private Polygon2D _arrow = null!;
    private bool _isRotating;
    private bool _clockwise;

    protected override Color BaseColor => new(0.25f, 0.4f, 0.5f);
    protected override Color BorderColor => new(0.35f, 0.5f, 0.6f);

    protected override void OnReady()
    {
        base.OnReady();
        _arrow = GetNode<Polygon2D>("Arrow");
    }

    public void Initialize(Core.Models.Position pos, Core.Models.TileType type)
    {
        base.Initialize(pos, type);
        _clockwise = type == Core.Models.TileType.ConveyorCW || type == Core.Models.TileType.ExpressConveyorCW;
        
        var dir = type switch
        {
            Core.Models.TileType.ConveyorCW or Core.Models.TileType.ExpressConveyorCW => 90f,
            Core.Models.TileType.ConveyorCCW or Core.Models.TileType.ExpressConveyorCCW => -90f,
            _ => 0f
        };
        
        if (_arrow != null)
            _arrow.RotationDegrees = dir;
    }

    public override void StartAnimations()
    {
        if (!IsInsideTree() || _arrow == null) return;
        if (_isRotating) return;
        _isRotating = true;
        
        float speed = (TileType == Core.Models.TileType.ExpressConveyorCW || TileType == Core.Models.TileType.ExpressConveyorCCW) ? 1.0f : 2.0f;
        float direction = _clockwise ? 360f : -360f;
        
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(_arrow, "rotation_degrees", direction / speed, speed);
    }

    public override void StopAnimations()
    {
        _isRotating = false;
    }
}
