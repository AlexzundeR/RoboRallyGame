namespace AiTest.Godot.Tests.Animations;

using Godot;
using AiTest.Godot;
using AiTest.Godot.Animations;
using AiTest.Core.Models;
using GdUnit4;

[TestSuite]
public class MoveAnimationTests
{
    [TestCase]
    [RequireGodotRuntime]
    public void MoveAnimation_ShouldAnimateFromCurrentPosition()
    {
        var robot = new Robot();
        robot.SetPosition(new Vector2(120, 120));
        
        var animation = new MoveAnimation
        {
            Robot = robot,
            To = new Position(3, 3),
            Duration = 0.1f
        };
        
        var context = new Node();
        
        var currentPositionBeforeAnimation = robot.Position;
        
        var task = animation.PlayAsync(context);
        task.Wait(50);
        
        var currentPosDuringAnimation = robot.Position;
        
        currentPosDuringAnimation.X.Should().BeGreaterThan(currentPositionBeforeAnimation.X);
        
        robot.QueueFree();
        context.QueueFree();
    }
}