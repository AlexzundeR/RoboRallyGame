using Godot;
using System.Collections.Generic;
using AiTest.Core.Entities;
using AiTest.Core.Models;

namespace AiTest.Godot;

public partial class RobotRenderer : Node2D
{
    private readonly Dictionary<int, ColorRect> _robotSprites = new();
    private readonly Dictionary<int, ColorRect> _directionIndicators = new();
    private const int TileSize = 48;
    private const int MapOffsetY = 120;

    public void CreateRobots(IEnumerable<Player> players)
    {
        foreach (var player in players)
        {
            CreateRobotSprite(player);
        }
    }

	private void CreateRobotSprite(Player player)
	{
		var robotSize = TileSize - 20;
		var robot = new ColorRect
		{
			Name = $"Robot_{player.Id}",
			Position = new Vector2(
				player.Robot.Position.X * TileSize + 10,
				player.Robot.Position.Y * TileSize + MapOffsetY + 10
			),
			Size = new Vector2(robotSize, robotSize),
			Color = player.Id == 1 ? Colors.DodgerBlue : Colors.Red,
			PivotOffset = new Vector2(robotSize / 2, robotSize / 2)
		};

		var directionIndicator = new ColorRect
		{
			Name = "DirectionIndicator",
			Size = new Vector2(12, 12),
			Color = Colors.Yellow
		};
		robot.AddChild(directionIndicator);
		_directionIndicators[player.Id] = directionIndicator;

		var healthBar = new ColorRect
		{
			Position = new Vector2(4, TileSize - 28),
			Size = new Vector2(TileSize - 28, 4),
			Color = Colors.Green
		};
		robot.AddChild(healthBar);

		AddChild(robot);
		_robotSprites[player.Id] = robot;
	}

	public void UpdatePositions(IEnumerable<Player> players)
	{
		foreach (var player in players)
		{
			if (_robotSprites.TryGetValue(player.Id, out var sprite))
			{
				var targetPos = new Vector2(
					player.Robot.Position.X * TileSize + 10,
					player.Robot.Position.Y * TileSize + MapOffsetY + 10
				);

				sprite.Position = sprite.Position.Lerp(targetPos, 0.15f);

				if (!player.Robot.IsAlive)
				{
					sprite.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
				}
			}
		}

		UpdateDirections(players);
	}

    private void UpdateDirections(IEnumerable<Player> players)
    {
        foreach (var player in players)
        {
            if (_directionIndicators.TryGetValue(player.Id, out var indicator))
            {
                var (offsetX, offsetY) = GetDirectionOffset(player.Robot.Direction);
                indicator.Position = new Vector2(
                    TileSize / 2 - 6 + offsetX * 15,
                    TileSize / 2 - 6 + offsetY * 15
                );
            }
        }
    }

    private (float, float) GetDirectionOffset(Direction direction)
    {
        return direction switch
        {
            Direction.North => (0, -1),
            Direction.East => (1, 0),
            Direction.South => (0, 1),
            Direction.West => (-1, 0),
            _ => (0, -1)
        };
    }
}
