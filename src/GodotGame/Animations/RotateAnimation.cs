namespace AiTest.Godot.Animations;

using System.Threading.Tasks;
using Godot;
using AiTest.Core.Models;

public class RotateAnimation : Animation
{
	public Robot Robot { get; init; } = null!;
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
		GD.Print($"Anime {Duration}");
		tween.SetTrans(Tween.TransitionType.Sine);
		tween.SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(Robot, "rotation_degrees", DirectionToAngle(To), Duration);
		tween.Finished += () =>
		{
		  GD.Print($"Anime finished {Duration}");  
		  tcs.SetResult(true);
		};  
		tween.Play();
		
		return tcs.Task;
	}

	private float DirectionToAngle(Direction dir) => dir switch
	{
		Direction.North => 0f,
		Direction.East => 90f,
		Direction.South => 180f,
		Direction.West => 270f,
		_ => 0f
	};
}
