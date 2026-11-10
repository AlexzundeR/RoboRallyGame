namespace RoboRallyGame.Godot.Animations;

using System.Threading.Tasks;
using Godot;

public class RotateAnimation : Animation
{
	public RobotNode Robot { get; init; } = null!;
	public int FromIndex { get; init; }
	public int ToIndex { get; init; }
	public float Duration { get; init; } = 0.4f;

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
		
		tween.SetTrans(Tween.TransitionType.Sine);
		tween.SetEase(Tween.EaseType.InOut);
		
		var toAngle = DirectionIndexToAngle(ToIndex);
		var targetAngle = CalculateShortestAngle(Robot.RotationDegrees, toAngle);
		tween.TweenProperty(Robot, "rotation_degrees", targetAngle, Duration);
		tween.Finished += () =>
		{
			Robot.SetDirection(ToIndex);
            Robot.SetAnimating(false);
            tcs.SetResult(true);
		};
		tween.Play();
		
		return tcs.Task;
	}

	private static float DirectionIndexToAngle(int dirIndex) => dirIndex switch
	{
		0 => 0f,
		1 => 90f,
		2 => 180f,
		3 => 270f,
		_ => 0f
	};

	private static float CalculateShortestAngle(float fromAngle, float toAngle)
	{
		var diff = toAngle - fromAngle;
		
		if (diff > 180) diff -= 360;
		if (diff < -180) diff += 360;
		
		return fromAngle + diff;
	}

	// private void AnimateRotation(int toIndex)
	// {
	// 	float targetAngle = DirectionIndexToAngle(toIndex);
	// 	float currentAngle = RotationDegrees;
		
	// 	float shortestAngle = CalculateShortestAngle(currentAngle, targetAngle);
		
	// 	if (System.Math.Abs(shortestAngle - currentAngle) < 1f)
	// 		return;
		
	// 	if (_rotationTween?.IsRunning() == true)
	// 		_rotationTween.Kill();
		
	// 	_rotationTween = CreateTween();
	// 	_rotationTween.SetTrans(Tween.TransitionType.Sine);
	// 	_rotationTween.SetEase(Tween.EaseType.InOut);
	// 	_rotationTween.TweenProperty(this, "rotation_degrees", shortestAngle, RotationDuration);
	// }
}
