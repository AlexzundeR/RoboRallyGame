namespace RoboRallyGame.Godot.Services;

using Godot;
using System;

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

    public static Vector2 GridToWorld(int gridX, int gridY)
    {
        var tileSize = Instance?.TileSize ?? BaseTileSize;
        return new Vector2(gridX * tileSize, gridY * tileSize);
    }

    public static Vector2 GridToWorldCentered(int gridX, int gridY)
    {
        var tileSize = Instance?.TileSize ?? BaseTileSize;
        var half = tileSize / 2f;
        return new Vector2(gridX * tileSize + half, gridY * tileSize + half);
    }

    public static float GetMapScale() => Instance?.ScaleFactor ?? 1f;

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
