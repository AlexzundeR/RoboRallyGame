namespace RoboRallyGame.Godot;

using System;

using Godot;

using RoboRallyGame.Core.Models;
using RoboRallyGame.Godot.Services;

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
    private bool _isRebooting;

    public int PlayerId => _playerId;

	public override void _Ready()
	{
		_body = GetNode<ColorRect>("Body");
		_directionIndicator = GetNode<Polygon2D>("DirectionIndicator");
		_healthBar = GetNode<ColorRect>("HealthBar");
		
		_directionIndicator.Color = Colors.Yellow;
	}

    internal void SetGridPosition(Position position)
    {
        _gridX = position.X;
		_gridY = position.Y;
    }

	public void Initialize(PlayerViewModel player)
	{
		_playerId = player.Id;
		//TODO: сделать цветовую схему игрока вместе с иконками в PlayerPanel отдельно
		_playerColor = player.Id switch
		{
			1 => Colors.DodgerBlue,
			2 => Colors.Red,
			3 => Colors.Yellow,
			4 => Colors.WhiteSmoke
		};
		_gridX = player.X;
		_gridY = player.Y;
		_directionIndex = (int)player.Direction;
		_damage = player.Damage;
		_isAlive = player.IsAlive;
		_isPowerDown = player.IsPowerDown;
		_isRebooting = player.IsRebooting;

		UpdateVisuals();
	}

	public void UpdateFromPlayer(PlayerViewModel player)
	{
		_gridX = player.X;
		_gridY = player.Y;
		_directionIndex = (int)player.Direction;
		_damage = player.Damage;
		_isAlive = player.IsAlive;
		_isPowerDown = player.IsPowerDown;
		_isRebooting = player.IsRebooting;

		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (!IsInsideTree()) return;
		
		if (!_isAnimating)
		{
			Position = GetWorldPosition();
			RotationDegrees = DirectionIndexToAngle(_directionIndex);
		}

		_body.Color = _playerColor;
		
		if (!_isAlive)
		{
			_body.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
			_directionIndicator.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
		}
		else if (_isPowerDown || _isRebooting)
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

	public int GetDirection() => _directionIndex;
	public bool IsAlive() => _isAlive;

	public Vector2 GetWorldPosition()
	{
		return new Position(_gridX, _gridY).ToTilePositionCentered();
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
		RotationDegrees = DirectionIndexToAngle(_directionIndex);
	}

	public void SetAnimating(bool animating)
	{
		_isAnimating = animating;
	}
}
