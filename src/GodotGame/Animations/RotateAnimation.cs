namespace AiTest.Godot.Animations;

using System.Threading.Tasks;
using Godot;
using AiTest.Core.Models;

public class RotateAnimation : Animation
{
    public ColorRect Robot { get; init; } = null!;
    public Direction To { get; init; }
    public float Duration { get; init; } = 0.4f;

    public override Task PlayAsync(Node context)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        var tween = context.CreateTween();
        if (tween == null)
        {
            tcs.SetResult(false);
            return tcs.Task;
        }
        
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(Robot, "rotation_degrees", AnimationService.DirectionToAngle(To), Duration);
        tween.Finished += () => tcs.SetResult(true);
        tween.Play();
        
        return tcs.Task;
    }
}
