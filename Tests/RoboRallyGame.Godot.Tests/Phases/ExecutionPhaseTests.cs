namespace RoboRallyGame.Godot.Tests.Phases;

using RoboRallyGame.Core.Config;
using RoboRallyGame.Core.Engine;
using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Messages;
using RoboRallyGame.Godot.Phases;
using Xunit;

public class ExecutionPhaseTests
{
    private readonly GameRunner _runner;
    private readonly MessageDispatcher _dispatcher;
    private readonly ExecutionPhase _phase;

    public ExecutionPhaseTests()
    {
        _runner = new GameRunner(TestConfigs.CreateDefault2Player());
        _runner.Initialize();
        _dispatcher = new MessageDispatcher();
        _phase = new ExecutionPhase();
        _phase.Initialize(TestHelpers.CreateProvider(_runner), _dispatcher, _ => { });
    }

    [Fact]
    public void Initialize_ShouldSetRunnerAndDispatcher()
    {
        Assert.NotNull(_phase.CurrentPlayerName);
    }

    [Fact]
    public void CurrentPlayerIndex_ShouldReturnValidIndex()
    {
        var index = _phase.CurrentPlayerIndex;
        Assert.InRange(index, 0, _runner.Engine.State.Players.Count - 1);
    }

    [Fact]
    public void CurrentPlayerName_ShouldReturnPlayerName()
    {
        var name = _phase.CurrentPlayerName;
        Assert.NotEmpty(name);
    }

    [Fact]
    public void CurrentRegister_ShouldReturnValidRegister()
    {
        var register = _phase.CurrentRegister;
        Assert.InRange(register, 0, 4);
    }

    [Fact]
    public void OnEnter_ShouldNotThrow()
    {
        _phase.OnEnter();
    }

    [Fact]
    public void OnExit_ShouldNotThrow()
    {
        _phase.OnExit();
    }

    [Fact]
    public void Update_ShouldNotThrow()
    {
        _phase.Update(0.016);
    }
}
