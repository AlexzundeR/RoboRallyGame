namespace AiTest.Godot;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using AiTest.Core.Engine;
using AiTest.Core.Messages;
using AiTest.Core.Models;

public partial class AnimationController : Node
{
    private GameRunner? _runner;
    private Node2D? _robotLayer;
    private Node2D? _effectsLayer;
    private bool _animationsEnabled = true;
    private RichTextLabel? _actionLog;
    private readonly List<string> _logMessages = new();
    private const int MaxLogMessages = 50;
    private bool _isWaitingForAnimation = false;

    private const int TileSize = 48;
    private const int MapOffsetY = 120;

    private const float PhaseDelay = 0.8f;
    private const float MoveDuration = 0.6f;
    private const float RotateDuration = 0.4f;
    private const float ConveyorDuration = 0.5f;
    private const float LaserDuration = 1.5f;
    private const float DamageFlashDuration = 0.5f;
    private const float CheckpointDuration = 1.2f;
    private const float DestroyDuration = 1.0f;

    public override void _Ready()
    {
        GD.Print("[AnimationController] Ready");
    }

    public void SetEffectsLayer(Node2D layer)
    {
        _effectsLayer = layer;
    }

    public void Initialize(GameRunner runner, Node2D robotLayer)
    {
        _runner = runner;
        _robotLayer = robotLayer;

        var engine = runner.Engine;
        engine.OnMove += OnMove;
        engine.OnRotate += OnRotate;
        engine.OnLaserFire += OnLaserFire;
        engine.OnDamage += OnDamage;
        engine.OnConveyorMove += OnConveyorMove;
        engine.OnCheckpoint += OnCheckpoint;
        engine.OnRobotDestroyed += OnRobotDestroyed;
        engine.OnPhase1Started += OnPhase1Started;
        engine.OnPhase2Started += OnPhase2Started;
        engine.OnPhase3Started += OnPhase3Started;
        
        GD.Print("[AnimationController] Subscribed to engine events");
    }

    private void OnPhase1Started(Phase1StartedEvent e)
    {
        AddLogMessage($"[b]--- Register {e.Register + 1} ---[/b]");
        AddLogMessage("[color=purple]Card Phase[/color]");
    }

    private void OnPhase2Started(Phase2StartedEvent e)
    {
        AddLogMessage("[color=yellow]Board Phase (Conveyors)[/color]");
    }

    private void OnPhase3Started(Phase3StartedEvent e)
    {
        AddLogMessage("[color=red]Laser Phase[/color]");
    }

    public void SetActionLog(RichTextLabel logLabel)
    {
        _actionLog = logLabel;
        GD.Print($"[AnimationController] ActionLog set: {logLabel != null}");
    }

    public void ClearLog()
    {
        _logMessages.Clear();
        _actionLog?.Clear();
        _actionLog?.AppendText("Log initialized...\n");
    }

    public void SetAnimationsEnabled(bool enabled)
    {
        _animationsEnabled = enabled;
    }

    private Node? GetRobotNode(int robotId)
    {
        if (_robotLayer == null) return null;
        return _robotLayer.GetNodeOrNull<Node>($"Robot_{robotId}");
    }

    private string GetRobotName(int robotId)
    {
        var player = _runner?.Engine.State.Players.FirstOrDefault(p => p.Id == robotId);
        return player?.Name ?? $"Robot {robotId}";
    }

    public void AddLogMessage(string message)
    {
        GD.Print($"[AnimationController] AddLogMessage: {message}");
        _logMessages.Add(message);
        if (_logMessages.Count > MaxLogMessages)
        {
            _logMessages.RemoveAt(0);
        }
        
        _actionLog?.AppendText(message + "\n");
        
        var parent = _actionLog?.GetParent();
        if (parent != null)
        {
            var scroll = parent as ScrollContainer;
            scroll?.SetVScroll(scroll.GetVScroll() + 1000);
        }
    }

    private void OnMove(MoveEvent e)
    {
        if (!_animationsEnabled) return;
        
        var robotName = GetRobotName(e.PlayerId);
        AddLogMessage($"[color=cyan]{robotName}[/color] moves from ({e.From.X},{e.From.Y}) to ({e.To.X},{e.To.Y})");
        
        var robot = GetRobotNode(e.PlayerId);
        if (robot is ColorRect robotRect)
        {
            AnimateMove(robotRect, e.From, e.To);
        }
    }

    private void OnRotate(RotateEvent e)
    {
        if (!_animationsEnabled) return;
        
        var robotName = GetRobotName(e.PlayerId);
        var fromStr = e.From.ToString();
        var toStr = e.To.ToString();
        AddLogMessage($"[color=cyan]{robotName}[/color] rotates {fromStr} -> {toStr}");
        
        var robot = GetRobotNode(e.PlayerId);
        if (robot is ColorRect robotRect)
        {
            AnimateRotate(robotRect, e.From, e.To);
        }
    }

    private void OnLaserFire(LaserFireEvent e)
    {
        if (!_animationsEnabled) return;
        
        var robotName = GetRobotName(e.SourceId);
        var hitMsg = e.HitPlayerId.HasValue ? $" hits [color=red]{GetRobotName(e.HitPlayerId.Value)}[/color]" : " misses";
        AddLogMessage($"[color=cyan]{robotName}[/color] fires laser{hitMsg}");
        
        CreateLaserBeam(e.Source, e.Direction, e.Path);
    }

    private void OnDamage(DamageEvent e)
    {
        if (!_animationsEnabled) return;
        
        var robotName = GetRobotName(e.PlayerId);
        AddLogMessage($"[color=red]{robotName}[/color] takes {e.Damage} damage (HP: {e.NewHealth})");
        
        var robot = GetRobotNode(e.PlayerId);
        if (robot is ColorRect robotRect)
        {
            FlashRobotRed(robotRect);
        }
    }

    private void OnConveyorMove(ConveyorMoveEvent e)
    {
        if (!_animationsEnabled) return;
        
        var robotName = GetRobotName(e.PlayerId);
        AddLogMessage($"[color=yellow]{robotName}[/color] pushed by conveyor to ({e.To.X},{e.To.Y})");
        
        var robot = GetRobotNode(e.PlayerId);
        if (robot is ColorRect robotRect)
        {
            AnimateConveyorMove(robotRect, e.From, e.To);
        }
    }

    private void OnCheckpoint(CheckpointEvent e)
    {
        if (!_animationsEnabled) return;
        
        var robotName = GetRobotName(e.PlayerId);
        AddLogMessage($"[color=green]{robotName}[/color] reached checkpoint {e.CheckpointNumber}");
        
        var robot = GetRobotNode(e.PlayerId);
        if (robot != null)
        {
            PlayCheckpointEffect(robot);
        }
    }

    private void OnRobotDestroyed(RobotDestroyedEvent e)
    {
        if (!_animationsEnabled) return;
        
        var robotName = GetRobotName(e.PlayerId);
        AddLogMessage($"[color=gray]{robotName}[/color] was destroyed!");
        
        var robot = GetRobotNode(e.PlayerId);
        if (robot is ColorRect robotRect)
        {
            PlayDestroyEffect(robotRect);
        }
    }

    private Vector2 PositionToVector(Position pos)
    {
        return new Vector2(
            pos.X * TileSize + 10,
            pos.Y * TileSize + MapOffsetY + 10
        );
    }

    private void AnimateMove(ColorRect robot, Position from, Position to)
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.InOut);
        
        var startPos = PositionToVector(from);
        var endPos = PositionToVector(to);
        
        robot.Position = startPos;
        tween.TweenProperty(robot, "position", endPos, MoveDuration);
        
        tween.Play();
    }

    private void AnimateRotate(ColorRect robot, Direction from, Direction to)
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Back);
        tween.SetEase(Tween.EaseType.Out);
        
        var fromAngle = DirectionAngle(from);
        var toAngle = DirectionAngle(to);
        
        var rotationDiff = NormalizeAngle(toAngle - fromAngle);
        
        robot.RotationDegrees = fromAngle;
        tween.TweenProperty(robot, "rotation_degrees", fromAngle + rotationDiff, RotateDuration);
        
        tween.Play();
    }

    private static float DirectionAngle(Direction dir) => dir switch
    {
        Direction.North => 0f,
        Direction.East => 90f,
        Direction.South => 180f,
        Direction.West => 270f,
        _ => 0f
    };

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    private void AnimateConveyorMove(ColorRect robot, Position from, Position to)
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Linear);
        
        var startPos = PositionToVector(from);
        var endPos = PositionToVector(to);
        
        robot.Position = startPos;
        tween.TweenProperty(robot, "position", endPos, ConveyorDuration);
        
        tween.Play();
    }

    private void FlashRobotRed(ColorRect robot)
    {
        var originalColor = robot.Color;
        
        var tween = CreateTween();
        tween.SetParallel(true);
        
        robot.Color = Colors.Red;
        tween.TweenInterval(0.2f);
        tween.TweenProperty(robot, "color", originalColor, DamageFlashDuration);
        
        tween.Play();
    }

    private void CreateLaserBeam(Position source, Direction direction, IReadOnlyList<Position> path)
    {
        var worldNode = _effectsLayer ?? _robotLayer;
        if (worldNode == null) return;
        
        var startPos = PositionToVector(source) + new Vector2(TileSize / 2, TileSize / 2);
        
        var points = new List<Vector2> { startPos };
        
        foreach (var pos in path)
        {
            points.Add(PositionToVector(pos) + new Vector2(TileSize / 2, TileSize / 2));
        }
        
        if (points.Count < 2)
        {
            var offset = direction switch
            {
                Direction.North => new Vector2(0, -TileSize * 2),
                Direction.South => new Vector2(0, TileSize * 2),
                Direction.East => new Vector2(TileSize * 2, 0),
                Direction.West => new Vector2(-TileSize * 2, 0),
                _ => new Vector2(0, -TileSize * 2)
            };
            points.Add(startPos + offset);
        }
        
        var endPos = points[points.Count - 1];
        
        var laserLine = new Line2D
        {
            Name = "LaserBeam",
            Width = 10,
            DefaultColor = new Color(1f, 0f, 0f, 1f),
            Antialiased = true,
            JointMode = Line2D.LineJointMode.Bevel
        };
        
        foreach (var pt in points)
        {
            laserLine.AddPoint(pt);
        }
        
        worldNode.AddChild(laserLine);
        
        var hitMarker = new ColorRect
        {
            Name = "LaserHit",
            Size = new Vector2(16, 16),
            Color = Colors.Yellow,
            Position = endPos - new Vector2(8, 8)
        };
        worldNode.AddChild(hitMarker);
        
        var tween = CreateTween();
        tween.SetParallel(true);
        
        tween.TweenInterval(0.5f);
        tween.TweenProperty(laserLine, "modulate:a", 0f, 1.0f);
        tween.TweenProperty(hitMarker, "modulate:a", 0f, 1.0f);
        tween.TweenCallback(Callable.From(() => 
        {
            laserLine.QueueFree();
            hitMarker.QueueFree();
        }));
        
        tween.Play();
    }

    private void PlayCheckpointEffect(Node robot)
    {
        var checkpointSprite = new ColorRect
        {
            Color = Colors.Yellow,
            Size = new Vector2(20, 20),
            Position = new Vector2(TileSize / 2 - 10, -30)
        };
        
        robot.AddChild(checkpointSprite);
        
        var tween = CreateTween();
        tween.SetParallel(true);
        
        tween.TweenProperty(checkpointSprite, "modulate:a", 0f, CheckpointDuration);
        tween.TweenProperty(checkpointSprite, "position:y", -60f, CheckpointDuration);
        tween.TweenCallback(Callable.From(() => checkpointSprite.QueueFree()));
        
        tween.Play();
    }

    private void PlayDestroyEffect(ColorRect robot)
    {
        var explosion = new ColorRect
        {
            Color = Colors.Orange,
            Size = new Vector2(TileSize, TileSize),
            Position = new Vector2(0, 0),
            Modulate = new Color(1, 1, 1, 0)
        };
        
        robot.AddChild(explosion);
        
        var tween = CreateTween();
        tween.SetParallel(true);
        
        explosion.Modulate = Colors.White;
        tween.TweenProperty(explosion, "scale", new Vector2(2, 2), DestroyDuration);
        tween.TweenProperty(explosion, "modulate:a", 0f, DestroyDuration);
        tween.TweenCallback(Callable.From(() => explosion.QueueFree()));
        
        robot.Modulate = Colors.DarkGray;
    }

    public override void _Process(double delta)
    {
    }
}
