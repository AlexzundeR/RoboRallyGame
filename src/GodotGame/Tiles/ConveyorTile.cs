namespace AiTest.Godot.Tiles;

using Godot;
using CoreModels = AiTest.Core.Models;

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

    public void Initialize(CoreModels.Position pos, CoreModels.TileType type, CoreModels.ConveyorDirection direction, bool isExpress)
    {
        base.Initialize(pos, type);
        _clockwise = direction is CoreModels.ConveyorDirection.North or CoreModels.ConveyorDirection.East;
        
        var rotation = direction switch
        {
            CoreModels.ConveyorDirection.North => 0f,
            CoreModels.ConveyorDirection.East => 90f,
            CoreModels.ConveyorDirection.South => 180f,
            CoreModels.ConveyorDirection.West => -90f,
            _ => 0f
        };
        
        if (_arrow != null)
            _arrow.RotationDegrees = rotation;
    }

    public override void StartAnimations()
    {
        if (!IsInsideTree() || _arrow == null) return;
        if (_isRotating) return;
        _isRotating = true;
        
        float speed = 1.0f;
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
