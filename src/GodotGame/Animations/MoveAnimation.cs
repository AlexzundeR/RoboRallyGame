namespace AiTest.Godot.Animations;

using System.Threading.Tasks;
using Godot;
using AiTest.Core.Models;

public class MoveAnimation : Animation
{
    public Robot Robot { get; init; } = null!;
    public Position From { get; init; }
    public Position To { get; init; }
    public float Duration { get; init; } = 0.6f;

    public override Task PlayAsync(Node context)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        Robot.SetAnimating(true);
        
        var fromVec = AnimationService.PositionToVector(From);
        var toVec = AnimationService.PositionToVector(To);
        Robot.Position = fromVec;
        
        var tween = context.CreateTween();
        if (tween == null)
        {
            Robot.SetAnimating(false);
            tcs.SetResult(false);
            return tcs.Task;
        }
        
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(Robot, "position", toVec, Duration);
        tween.Finished += () => 
        {
            Robot.SetAnimating(false);
            tcs.SetResult(true);
        };
        tween.Play();
        
        return tcs.Task;
    }
}
