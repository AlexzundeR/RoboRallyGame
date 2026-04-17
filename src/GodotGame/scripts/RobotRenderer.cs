using Godot;
using System.Collections.Generic;
using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Godot.Services;

namespace AiTest.Godot;

public partial class RobotRenderer : Node2D
{
    private readonly Dictionary<int, RobotNode> _robots = new();
    private PackedScene _robotScene = null!;

    public override void _Ready()
    {
        _robotScene = GD.Load<PackedScene>("res://RobotNode.tscn");
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
        var robot = _robotScene.Instantiate<RobotNode>();
        robot.Name = $"Robot_{player.Id}";
        AddChild(robot);
        robot.Initialize(player);
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

    public RobotNode? GetRobot(int playerId)
    {
        return _robots.TryGetValue(playerId, out var robot) ? robot : null;
    }
}
