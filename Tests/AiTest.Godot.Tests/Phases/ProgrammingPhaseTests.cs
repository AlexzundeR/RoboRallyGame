namespace AiTest.Godot.Tests.Phases;

using AiTest.Core.Cards;
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

    public bool IsActionJustPressed(string action) => _actions.GetValueOrDefault(action, false);
    public bool IsActionPressed(string action) => _actions.GetValueOrDefault(action, false);
}

public class ProgrammingPhaseTests
{
    private static Player CreateWithHand(params ProgramCard[] cards)
    {
        var hand = new ProgramCard?[9];
        for (int i = 0; i < cards.Length && i < 9; i++)
            hand[i] = cards[i];
        return new Player(null, hand);
    }

    private readonly GameRunner _runner;
    private readonly MessageDispatcher _dispatcher;
    private readonly ProgrammingPhase _phase;

    public ProgrammingPhaseTests()
    {
        _runner = new GameRunner(TestConfigs.CreateDefault2Player());
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
        var player = CreateWithHand(new MoveCard { Priority = 999, Spaces = 1 });
        var state = new GameState(new List<Player> { player });
        var runner = new GameRunner(new GameEngine(new RandomDealer(), state));
        var phase = new ProgrammingPhase();
        phase.Initialize(runner, _dispatcher, _ => { });
        
        var initialCount = player.AvailableCardCount;
        
        phase.SelectCard(0, 2);
        
        var registerCard = player.GetRegister(2);
        Assert.NotNull(registerCard);
        Assert.Equal(999, registerCard.Card.Priority);
        Assert.Equal(0, registerCard.HandIndex);
        Assert.Equal(initialCount - 1, player.AvailableCardCount);
    }

    [Fact]
    public void SelectCard_ShouldAdvanceSelectedRegister()
    {
        var player = CreateWithHand(new MoveCard { Priority = 999, Spaces = 1 });
        var state = new GameState(new List<Player> { player });
        var runner = new GameRunner(new GameEngine(new RandomDealer(), state));
        var phase = new ProgrammingPhase();
        phase.Initialize(runner, _dispatcher, _ => { });
        
        Assert.Equal(0, phase.SelectedRegister);
        phase.SelectCard(0, 0);
        
        Assert.Equal(1, phase.SelectedRegister);
    }

    [Fact]
    public void ConfirmPlayer_ShouldResetSelectedRegister()
    {
        var player = CreateWithHand(new MoveCard { Priority = 999, Spaces = 1 });
        var state = new GameState(new List<Player> { player });
        var runner = new GameRunner(new GameEngine(new RandomDealer(), state));
        var phase = new ProgrammingPhase();
        phase.Initialize(runner, _dispatcher, _ => { });
        
        phase.SelectCard(0, 3);
        Assert.Equal(1, phase.SelectedRegister);
        
        phase.ConfirmPlayer();
        
        Assert.Equal(0, phase.SelectedRegister);
    }
}
