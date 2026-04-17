namespace AiTest.Godot.Animations;

using System.Threading.Tasks;
using Godot;
using AiTest.Core.Models;

public class RotateAnimation : Animation
{
	public RobotNode Robot { get; init; } = null!;
	public Direction From { get; init; }
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
		
		tween.SetTrans(Tween.TransitionType.Sine);
		tween.SetEase(Tween.EaseType.InOut);
		
		var targetAngle = CalculateShortestAngle(From, To);
		tween.TweenProperty(Robot, "rotation_degrees", targetAngle, Duration);
		tween.Finished += () => tcs.SetResult(true);
		tween.Play();
		
		return tcs.Task;
	}

	private static float DirectionToAngle(Direction dir) => dir switch
	{
		Direction.North => 0f,
		Direction.East => 90f,
		Direction.South => 180f,
		Direction.West => 270f,
		_ => 0f
	};

	private static float CalculateShortestAngle(Direction from, Direction to)
	{
		var fromAngle = DirectionToAngle(from);
		var toAngle = DirectionToAngle(to);
		var diff = toAngle - fromAngle;
		
		if (diff > 180) diff -= 360;
		if (diff < -180) diff += 360;
		
		return fromAngle + diff;
	}
}
