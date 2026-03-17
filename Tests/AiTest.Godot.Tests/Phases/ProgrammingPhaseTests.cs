namespace AiTest.Godot.Tests.Phases;

using AiTest.Core.Engine;
using AiTest.Core.Messages;
using AiTest.Godot.Phases;
using Xunit;

public class ProgrammingPhaseTests
{
    private readonly GameRunner _runner;
    private readonly MessageDispatcher _dispatcher;
    private readonly ProgrammingPhase _phase;

    public ProgrammingPhaseTests()
    {
        _runner = new GameRunner(GameRunner.CreateMinimal());
        _runner.Initialize();
        _dispatcher = new MessageDispatcher();
        _phase = new ProgrammingPhase();
        _phase.Initialize(_runner, _dispatcher, _ => { });
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
        Assert.Equal(_runner.Engine.State.Players[_phase.CurrentPlayerIndex].Name, name);
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
