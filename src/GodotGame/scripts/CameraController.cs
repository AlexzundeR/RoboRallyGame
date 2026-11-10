namespace RoboRallyGame.Godot;

using Godot;
using RoboRallyGame.Godot.Services;

public partial class CameraController : Camera2D
{
    [Export] private float _zoomSpeed = 0.1f;
    [Export] private float _minZoom = 0.1f;
    [Export] private float _maxZoom = 5f;

    private Vector2 _dragStart;
    private bool _isDragging;
    private bool _dragEnabled = true;

	private Vector2 _dragStartMousePos;
	private Vector2 _dragStartCameraPos;


    public override void _Ready()
    {
        Zoom = new Vector2(0.8f, 0.8f);
    }

    public override void _Input(InputEvent @event)
    {
        HandleZoom(@event);
        HandlePan(@event);
    }

    private void HandleZoom(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                ApplyZoom(_zoomSpeed);
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                ApplyZoom(-_zoomSpeed);
            }
        }
    }

    private void ApplyZoom(float delta)
    {
        var newZoom = Zoom + new Vector2(delta, delta);
        newZoom.X = Mathf.Clamp(newZoom.X, _minZoom, _maxZoom);
        newZoom.Y = Mathf.Clamp(newZoom.Y, _minZoom, _maxZoom);
        Zoom = newZoom;
    }

    private void HandlePan(InputEvent @event)
    {
        if (!_dragEnabled)
            return;

        if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left || mouseButton.ButtonIndex == MouseButton.Middle)
			{
				if (mouseButton.Pressed)
				{
					_isDragging = true;
					_dragStartMousePos = GetViewport().GetMousePosition();
					_dragStartCameraPos = Position;
				}
				else
				{
					_isDragging = false;
				}
			}
		}

		if (@event is InputEventMouseMotion motion && _isDragging)
		{
			var currentMousePos = GetViewport().GetMousePosition();
			var delta = currentMousePos - _dragStartMousePos;
			Position = _dragStartCameraPos - delta / Zoom;
		}
    }

    public override void _Process(double delta)
    {
        HandleKeyboardPan(delta);
    }

    private void HandleKeyboardPan(double delta)
    {
        Vector2 moveDir = Vector2.Zero;

        if (Input.IsKeyPressed(Key.W))
            moveDir.Y -= 1;
        if (Input.IsKeyPressed(Key.S))
            moveDir.Y += 1;
        if (Input.IsKeyPressed(Key.A))
            moveDir.X -= 1;
        if (Input.IsKeyPressed(Key.D))
            moveDir.X += 1;
        if (Input.IsKeyPressed(Key.Q))
            ApplyZoom(-_zoomSpeed);
        if (Input.IsKeyPressed(Key.E))
            ApplyZoom(_zoomSpeed);

        if (moveDir != Vector2.Zero)
        {
            var panSpeed = GameScale.Instance?.PanSpeed ?? 400f;
            Position -= moveDir.Normalized() * panSpeed * (float)delta;
        }
    }

    public void CenterOn(Vector2 worldPosition)
    {
        Position = worldPosition;
    }

    public void SetDragEnabled(bool enabled)
    {
        _dragEnabled = enabled;
    }
}
