namespace AiTest.Godot;

using Godot;
using AiTest.Godot.Services;

public partial class RobotNode : Node2D
{
	private const float RotationDuration = 0.35f;

	private ColorRect _body = null!;
	private Polygon2D _directionIndicator = null!;
	private ColorRect _healthBar = null!;
	private Tween? _rotationTween;
	private bool _isAnimating;
	
	private int _playerId;
	private Color _playerColor;
	private int _gridX;
	private int _gridY;
	private int _directionIndex;
	private int _damage;
	private bool _isAlive = true;
	private bool _isPowerDown = false;

	public int PlayerId => _playerId;

	public override void _Ready()
	{
		_body = GetNode<ColorRect>("Body");
		_directionIndicator = GetNode<Polygon2D>("DirectionIndicator");
		_healthBar = GetNode<ColorRect>("HealthBar");
		
		_directionIndicator.Color = Colors.Yellow;
	}

	public void Initialize(PlayerViewModel player)
	{
		_playerId = player.Id;
		_playerColor = player.Id == 1 ? Colors.DodgerBlue : Colors.Red;
		_gridX = player.X;
		_gridY = player.Y;
		_directionIndex = (int)player.Direction;
		_damage = player.Damage;
		_isAlive = player.IsAlive;
		_isPowerDown = player.IsPowerDown;

		if (IsInsideTree())
		{
			Position = GetWorldPosition();
			RotationDegrees = DirectionIndexToAngle(_directionIndex);
			UpdateVisuals();
		}
	}

	public void UpdateFromPlayer(PlayerViewModel player)
	{
		_gridX = player.X;
		_gridY = player.Y;
		_directionIndex = (int)player.Direction;
		_damage = player.Damage;
		_isAlive = player.IsAlive;
		_isPowerDown = player.IsPowerDown;

		if (IsInsideTree())
			UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (!IsInsideTree()) return;
		
		if (!_isAnimating)
			Position = GetWorldPosition();

		AnimateRotation(_directionIndex);

		_body.Color = _playerColor;
		
		if (!_isAlive)
		{
			_body.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
			_directionIndicator.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
		}
		else if (_isPowerDown)
		{
			_body.Modulate = new Color(0.5f, 0.5f, 0.6f, 0.8f);
			_directionIndicator.Modulate = new Color(0.5f, 0.5f, 0.6f, 0.8f);
		}
		else
		{
			_body.Modulate = Colors.White;
			_directionIndicator.Modulate = Colors.White;
		}

		UpdateHealthBar();
	}

	private void AnimateRotation(int toIndex)
	{
		float targetAngle = DirectionIndexToAngle(toIndex);
		float currentAngle = RotationDegrees;
		
		float shortestAngle = CalculateShortestAngle(currentAngle, targetAngle);
		
		if (System.Math.Abs(shortestAngle - currentAngle) < 1f)
			return;
		
		if (_rotationTween?.IsRunning() == true)
			_rotationTween.Kill();
		
		_rotationTween = CreateTween();
		_rotationTween.SetTrans(Tween.TransitionType.Sine);
		_rotationTween.SetEase(Tween.EaseType.InOut);
		_rotationTween.TweenProperty(this, "rotation_degrees", shortestAngle, RotationDuration);
	}

	private float CalculateShortestAngle(float from, float to)
	{
		float diff = to - from;
		
		while (diff > 180) diff -= 360;
		while (diff < -180) diff += 360;
		
		return from + diff;
	}

	private void UpdateHealthBar()
	{
		if (_healthBar == null) return;
		
		float healthPercent = System.Math.Max(0, 1f - _damage / 10f);
		_healthBar.Size = new Vector2(22 * healthPercent, 2);
		
		if (healthPercent > 0.6f)
			_healthBar.Color = Colors.Green;
		else if (healthPercent > 0.3f)
			_healthBar.Color = Colors.Yellow;
		else
			_healthBar.Color = Colors.Red;
	}

	public void SetDirection(int directionIndex)
	{
		_directionIndex = directionIndex;
	}

	public (int X, int Y) GetGridPosition() => (_gridX, _gridY);
	public int GetDirection() => _directionIndex;
	public bool IsAlive() => _isAlive;

	public Vector2 GetWorldPosition()
	{
		var tileSize = GameScale.BaseTileSize;
		var halfTile = tileSize / 2f;
		return new Vector2(
			_gridX * tileSize + halfTile,
			_gridY * tileSize + halfTile
		);
	}

	private float DirectionIndexToAngle(int dirIndex) => dirIndex switch
	{
		0 => 0f,
		1 => 90f,
		2 => 180f,
		3 => 270f,
		_ => 0f
	};

	public override void _Process(double delta)
	{
		if (!IsInsideTree()) return;
		if (_isAnimating) return;
		
		Position = GetWorldPosition();
	}

	public void SetAnimating(bool animating)
	{
		_isAnimating = animating;
	}
}
