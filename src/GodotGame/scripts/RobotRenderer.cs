using Godot;
using System.Collections.Generic;
using AiTest.Core.Entities;
using AiTest.Core.Models;

namespace AiTest.Godot;

public partial class RobotRenderer : Node2D
{
    private readonly Dictionary<int, Robot> _robots = new();
    private PackedScene _robotScene = null!;
    private const int TileSize = 48;
    private const int MapOffsetY = 120;

    public override void _Ready()
    {
        _robotScene = GD.Load<PackedScene>("res://Robot.tscn");
    }

    public void CreateRobots(IEnumerable<Player> players)
    {
        foreach (var player in players)
        {
            CreateRobot(player);
        }
    }

    private void CreateRobot(Player player)
    {
        var robot = _robotScene.Instantiate<Robot>();
        robot.Initialize(player);
        robot.Name = $"Robot_{player.Id}";
        AddChild(robot);
        _robots[player.Id] = robot;
    }

    public void UpdatePositions(IEnumerable<Player> players)
    {
        foreach (var player in players)
        {
            if (_robots.TryGetValue(player.Id, out var robot))
            {
                robot.UpdateFromPlayer(player);
            }
        }
    }

    public Robot? GetRobot(int playerId)
    {
        return _robots.TryGetValue(playerId, out var robot) ? robot : null;
    }
}
