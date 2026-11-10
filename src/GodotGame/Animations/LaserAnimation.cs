namespace RoboRallyGame.Godot.Animations;

using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Godot.Services;

public class LaserAnimation : Animation
{
    public Position Source { get; init; }
    public Direction Direction { get; init; }
    public IReadOnlyList<Position> Path { get; init; } = new List<Position>();
    public float Duration { get; init; } = 2.0f;

    public override Task PlayAsync(Node context)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        if (Path.Count == 0)
        {
            tcs.SetResult(true);
            return tcs.Task;
        }
        
        var points = new List<Vector2>();
        points.Add(Source.ToTilePositionCentered());
        
        foreach (var pos in Path)
        {
            points.Add(pos.ToTilePositionCentered());
        }

        var endPos = points[points.Count - 1];

        var laserLine = new Line2D
        {
            Width = 10,
            DefaultColor = new Color(1f, 0f, 0f, 1f),
            Antialiased = true,
            JointMode = Line2D.LineJointMode.Bevel
        };

        foreach (var pt in points)
        {
            laserLine.AddPoint(pt);
        }

        context.AddChild(laserLine);

        var hitMarker = new ColorRect
        {
            Size = new Vector2(16, 16),
            Color = Colors.Yellow,
            Position = endPos - new Vector2(8, 8)
        };
        context.AddChild(hitMarker);

        var tween = context.CreateTween();
        if (tween == null)
        {
            laserLine.QueueFree();
            hitMarker.QueueFree();
            tcs.SetResult(false);
            return tcs.Task;
        }

        tween.SetParallel(true);
        tween.TweenInterval(0.5f);
        tween.TweenProperty(laserLine, "modulate:a", 0f, 1.0f);
        tween.TweenProperty(hitMarker, "modulate:a", 0f, 1.0f);
        tween.Finished += () =>
        {
            laserLine.QueueFree();
            hitMarker.QueueFree();
            tcs.SetResult(true);
        };
        tween.Play();
        
        return tcs.Task;
    }
}
