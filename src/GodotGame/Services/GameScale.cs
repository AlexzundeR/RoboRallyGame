namespace AiTest.Godot.Services;

using Godot;

public partial class GameScale : Node
{
    public static GameScale? Instance { get; private set; }

    public const float BaseViewportWidth = 1280f;
    public const float BaseViewportHeight = 720f;
    public const float BaseTileSize = 48f;
    public const float BaseActionLogWidth = 280f;
    public const float BasePanSpeed = 400f;

    private float _scaleFactor = 1f;
    private Vector2 _viewportSize;

    public float ScaleFactor => _scaleFactor;
    public float TileSize => BaseTileSize * _scaleFactor;
    public float ActionLogWidth => BaseActionLogWidth * _scaleFactor;
    public float PanSpeed => BasePanSpeed * _scaleFactor;
    public Vector2 ViewportSize => _viewportSize;

    public override void _Ready()
    {
        Instance = this;
        Recalculate();
        GetTree().Root.SizeChanged += OnViewportSizeChanged;
    }

    private void OnViewportSizeChanged()
    {
        Recalculate();
    }

    public void Recalculate()
    {
        var rootWindow = GetTree().Root;
        _viewportSize = rootWindow.Size;

        var scaleX = _viewportSize.X / BaseViewportWidth;
        var scaleY = _viewportSize.Y / BaseViewportHeight;

        _scaleFactor = Mathf.Min(scaleX, scaleY);
        _scaleFactor = Mathf.Max(_scaleFactor, 0.5f);
    }
}
