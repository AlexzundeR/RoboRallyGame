namespace AiTest.Godot.Tests.Phases;

using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Messages;
using AiTest.Godot.Phases;
using Xunit;

public class MockInputService : IInputService
{
    private readonly Dictionary<string, bool> _actions = new();

    public void SetAction(string action, bool pressed)
    {
        _actions[action] = pressed;
    }

    public bool IsActionJustPressed(string action)
    {
        if (_actions.TryGetValue(action, out var pressed) && pressed)
        {
            _actions[action] = false;
            return true;
        }
        return false;
    }
}

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
    public void SelectedRegister_ShouldReturnZeroByDefault()
    {
        Assert.Equal(0, _phase.SelectedRegister);
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

    [Fact]
    public void SelectCard_ShouldPlaceCardInSelectedRegister()
    {
        var player = _runner.Engine.State.Players[_phase.CurrentPlayerIndex];
        var card = new MoveCard { Priority = 999, Spaces = 1 };
        player.Hand[0] = card;
        var initialCount = player.AvailableCardCount;
        
        _phase.SelectCard(0, 2);
        
        Assert.NotNull(player.GetRegister(2));
        Assert.Same(card, player.GetRegister(2));
        Assert.Equal(initialCount - 1, player.AvailableCardCount);
    }

    [Fact]
    public void SelectCard_ShouldAdvanceSelectedRegister()
    {
        var player = _runner.Engine.State.Players[_phase.CurrentPlayerIndex];
        player.Hand[0] = new MoveCard { Priority = 999, Spaces = 1 };
        
        Assert.Equal(0, _phase.SelectedRegister);
        _phase.SelectCard(0, 0);
        
        Assert.Equal(1, _phase.SelectedRegister);
    }

    [Fact]
    public void ConfirmPlayer_ShouldResetSelectedRegister()
    {
        var player = _runner.Engine.State.Players[_phase.CurrentPlayerIndex];
        player.Hand[0] = new MoveCard { Priority = 999, Spaces = 1 };
        
        _phase.SelectCard(0, 3);
        Assert.Equal(1, _phase.SelectedRegister);
        
        _phase.ConfirmPlayer();
        
        Assert.Equal(0, _phase.SelectedRegister);
    }
}
