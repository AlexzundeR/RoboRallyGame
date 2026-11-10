namespace RoboRallyGame.Godot.Animations;

using System.Threading.Tasks;
using Godot;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Godot.Services;

public class MoveAnimation : Animation
{
    public RobotNode Robot { get; init; } = null!;
    public Position GridFrom { get; init; }
    public Position GridTo { get; init; }
    public float Duration { get; init; } = 0.6f;

    public override Task PlayAsync(Node context)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        Robot.SetAnimating(true);
        
        Robot.Position = GridFrom.ToTilePositionCentered();
        
        var tween = context.CreateTween();
        if (tween == null)
        {
            Robot.SetAnimating(false);
            tcs.SetResult(false);
            return tcs.Task;
        }
        
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(Robot, "position", GridTo.ToTilePositionCentered(), Duration);
        tween.Finished += () => 
        {
            Robot.SetGridPosition(GridTo);
            Robot.SetAnimating(false);
            tcs.SetResult(true);
        };
        tween.Play();
        
        return tcs.Task;
    }
}
