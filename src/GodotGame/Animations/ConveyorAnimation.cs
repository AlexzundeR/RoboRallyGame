namespace AiTest.Godot.Animations;

using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using AiTest.Core.Models;

public class ConveyorAnimation : Animation
{
    public ColorRect Robot { get; init; } = null!;
    public Position From { get; init; }
    public Position To { get; init; }
    public float Duration { get; init; } = 0.5f;

    public override Task PlayAsync(Node context)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        Robot.Position = AnimationService.PositionToVector(From);
        
        var tween = context.CreateTween();
        if (tween == null)
        {
            tcs.SetResult(false);
            return tcs.Task;
        }
        
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(Robot, "position", AnimationService.PositionToVector(To), Duration);
        tween.Finished += () => tcs.SetResult(true);
        tween.Play();
        
        return tcs.Task;
    }
}
