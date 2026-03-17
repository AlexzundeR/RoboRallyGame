namespace AiTest.Godot.Tests.GameFlow;

using AiTest.Core.Engine;
using AiTest.Core.Messages;
using AiTest.Godot.Phases;
using Xunit;

public class GameFlowTests
{
    [Fact]
    public void ProgrammingPhase_ToExecutionPhase_ShouldPublishPhaseChangedEvent()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var phaseChangedEvent = false;
        GamePhaseType? receivedPhase = null;
        int? receivedRound = null;

        dispatcher.Subscribe<PhaseChangedEvent>(e =>
        {
            phaseChangedEvent = true;
            receivedPhase = e.Phase;
            receivedRound = e.Round;
        });

        var programmingPhase = new ProgrammingPhase();
        var executionPhase = new ExecutionPhase();
        GamePhaseType nextPhase = GamePhaseType.Programming;

        void OnPhaseComplete(GamePhaseType newPhase)
        {
            nextPhase = newPhase;
            dispatcher.Publish(new PhaseChangedEvent(newPhase, 1));
        }

        programmingPhase.Initialize(runner, dispatcher, OnPhaseComplete);
        executionPhase.Initialize(runner, dispatcher, OnPhaseComplete);

        programmingPhase.OnEnter();
        OnPhaseComplete(GamePhaseType.Execution);

        Assert.True(phaseChangedEvent);
        Assert.Equal(GamePhaseType.Execution, receivedPhase);
        Assert.Equal(1, receivedRound);
    }

    [Fact]
    public void ExecutionPhase_ToProgrammingPhase_ShouldIncrementRound()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var events = new List<PhaseChangedEvent>();

        dispatcher.Subscribe<PhaseChangedEvent>(e => events.Add(e));

        var programmingPhase = new ProgrammingPhase();
        var executionPhase = new ExecutionPhase();
        int round = 1;

        void OnPhaseComplete(GamePhaseType newPhase)
        {
            if (newPhase == GamePhaseType.Programming)
            {
                round++;
            }
            dispatcher.Publish(new PhaseChangedEvent(newPhase, round));
        }

        programmingPhase.Initialize(runner, dispatcher, OnPhaseComplete);
        executionPhase.Initialize(runner, dispatcher, OnPhaseComplete);

        executionPhase.OnEnter();
        OnPhaseComplete(GamePhaseType.Programming);

        Assert.Equal(2, round);
        Assert.Contains(events, e => e.Phase == GamePhaseType.Programming && e.Round == 2);
    }

    [Fact]
    public void TwoPlayerGame_FullProgrammingPhase_ShouldTrackCurrentPlayer()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var phase = new ProgrammingPhase();
        phase.Initialize(runner, dispatcher, _ => { });

        var firstPlayer = phase.CurrentPlayerName;
        runner.Engine.State.CurrentPlayerIndex = 1;
        var secondPlayer = phase.CurrentPlayerName;

        Assert.NotEqual(firstPlayer, secondPlayer);
        Assert.Equal("Player1", firstPlayer);
        Assert.Equal("Player2", secondPlayer);
    }

    [Fact]
    public void TwoPlayerGame_Registers_ShouldStartAt0()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var phase = new ProgrammingPhase();
        phase.Initialize(runner, dispatcher, _ => { });

        Assert.Equal(0, phase.CurrentRegister);
    }

    [Fact]
    public void ProgrammingPhase_AfterCardSelection_ShouldPublishCardSelectedEvent()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var cardSelectedEvents = new List<CardSelectedEvent>();

        dispatcher.Subscribe<CardSelectedEvent>(e => cardSelectedEvents.Add(e));

        var phase = new ProgrammingPhase();
        phase.Initialize(runner, dispatcher, _ => { });

        dispatcher.Publish(new CardSelectedEvent(0, 3, 0));

        Assert.Single(cardSelectedEvents);
        Assert.Equal(0, cardSelectedEvents[0].PlayerIndex);
        Assert.Equal(3, cardSelectedEvents[0].CardIndex);
        Assert.Equal(0, cardSelectedEvents[0].Register);
    }

    [Fact]
    public void ExecutionPhase_AfterRegisterExecution_ShouldPublishRegisterExecutedEvent()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var registerExecutedEvents = new List<RegisterExecutedEvent>();

        dispatcher.Subscribe<RegisterExecutedEvent>(e => registerExecutedEvents.Add(e));

        var phase = new ExecutionPhase();
        phase.Initialize(runner, dispatcher, _ => { });

        dispatcher.Publish(new RegisterExecutedEvent(0, 1));

        Assert.Single(registerExecutedEvents);
        Assert.Equal(0, registerExecutedEvents[0].Register);
        Assert.Equal(1, registerExecutedEvents[0].PlayerIndex);
    }

    [Fact]
    public void PlayerConfirmation_ShouldPublishPlayerConfirmedEvent()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var confirmedEvents = new List<PlayerConfirmedEvent>();

        dispatcher.Subscribe<PlayerConfirmedEvent>(e => confirmedEvents.Add(e));

        var phase = new ProgrammingPhase();
        phase.Initialize(runner, dispatcher, _ => { });

        dispatcher.Publish(new PlayerConfirmedEvent(0));

        Assert.Single(confirmedEvents);
        Assert.Equal(0, confirmedEvents[0].PlayerIndex);
    }
}
