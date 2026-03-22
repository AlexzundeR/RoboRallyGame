namespace AiTest.Godot.Animations;

using System.Threading.Tasks;
using Godot;
using AiTest.Core.Models;

public class MoveAnimation : Animation
{
    public Robot Robot { get; init; } = null!;
    public Position To { get; init; }
    public float Duration { get; init; } = 0.6f;

    private const int TileSize = 48;
    private const int MapOffsetY = 120;

    public override Task PlayAsync(Node context)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        Robot.SetAnimating(true);
        
        var tween = context.CreateTween();
        if (tween == null)
        {
            Robot.SetAnimating(false);
            tcs.SetResult(false);
            return tcs.Task;
        }
        
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(Robot, "position", PositionToVector(To), Duration);
        tween.Finished += () => 
        {
            Robot.SetAnimating(false);
            tcs.SetResult(true);
        };
        tween.Play();
        
        return tcs.Task;
    }

    private Vector2 PositionToVector(Position pos)
    {
        return new Vector2(pos.X * TileSize + 24, pos.Y * TileSize + MapOffsetY + 24);
    }
}
