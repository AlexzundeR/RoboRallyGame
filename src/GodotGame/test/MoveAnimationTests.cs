namespace RoboRallyGame.Tests.GdUnit.Animations;

using RoboRallyGame.Godot.Animations;
using RoboRallyGame.Core.Models;
using GdUnit4;
using static GdUnit4.Assertions;
using Godot;

using RobotNode = Godot.RobotNode;

using System.Threading.Tasks;

using CommandLine;
using RoboRallyGame.Godot.Services;

[TestSuite]
[RequireGodotRuntime]
public class MoveAnimationTests
{
    private ISceneRunner runner = null!;

    [BeforeTest]
    public void TestSetup()
    {
        
        runner = ISceneRunner.Load("res://RobotNode.tscn", true);

        // We maximize the view to bring the window to foreground to see what actually happened in the scene.
        runner.MaximizeView();
    }

    [TestCase]
    public async Task MoveAnimation_ShouldCreate_WithRobotAndPositions()
    {
        var robot = runner.Scene().Cast<RobotNode>();
        
        var animation = new MoveAnimation
        {
            Robot = robot,
            GridFrom = new Position(0, 0),
            GridTo = new Position(3, 3),
            Duration = 0.6f
        };

        AssertThat(robot.Position.X).IsEqual(0);
        
        AssertThat(animation.Robot).IsNotNull();
        AssertThat(animation.GridFrom).IsEqual(new Position(0, 0));
        AssertThat(animation.GridTo).IsEqual(new Position(3, 3));
        AssertThat(animation.Duration).IsEqual(0.6f);
        animation.PlayAsync(robot).ConfigureAwait(false);
        await runner.SimulateFrames(100);

        AssertThat(robot.Position).IsEqual(new Position(3,3).ToTilePositionCentered());

        robot.QueueFree();
    }

    [TestCase]
    public async Task ConveyorAnimation_ShouldCreate_WithRobotAndPositions()
    {
        var robot = runner.Scene().Cast<RobotNode>();
        
        var animation = new ConveyorAnimation
        {
            Robot = robot,
            GridFrom = new Position(0, 0),
            GridTo = new Position(3, 3),
            Duration = 0.6f
        };

        AssertThat(robot.Position.X).IsEqual(0);
        
        AssertThat(animation.Robot).IsNotNull();
        AssertThat(animation.GridFrom).IsEqual(new Position(0, 0));
        AssertThat(animation.GridTo).IsEqual(new Position(3, 3));
        AssertThat(animation.Duration).IsEqual(0.6f);
        animation.PlayAsync(robot).ConfigureAwait(false);
        await runner.SimulateFrames(100);

        robot.QueueFree();
    }
}
