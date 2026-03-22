namespace AiTest.Godot.Tests.ExecutionPhase;

using System.Collections.Generic;
using AiTest.Core.Cards;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Messages;
using AiTest.Core.Models;
using AiTest.Godot.Animations;
using AiTest.Godot.Phases;
using Xunit;

public class ExecutionPhaseIntegrationTests
{
    private static Player CreatePlayerWithCards(params ProgramCard[] cards)
    {
        var registers = new RegisterCard?[5];
        var hand = new ProgramCard?[9];
        for (int i = 0; i < cards.Length && i < 9; i++)
        {
            hand[i] = cards[i];
        }
        return new Player(registers, hand);
    }

    private static GameRunner CreateRunnerWithCards(params ProgramCard[] cards)
    {
        var player = CreatePlayerWithCards(cards);
        var runner = new GameRunner(new GameEngine(
            new RandomDealer(),
            new GameState(new List<Player> { player })));
        runner.Initialize();
        return runner;
    }

    [Fact]
    public void ExecutionPhase_ExecuteRegister_PublishesRegisterExecutedEvent()
    {
        var runner = CreateRunnerWithCards(new MoveCard { Priority = 100, Spaces = 1 });
        var dispatcher = new MessageDispatcher();
        var animationService = new MockAnimationService();

        var events = new List<RegisterExecutedEvent>();
        dispatcher.Subscribe<RegisterExecutedEvent>(e => events.Add(e));

        var phase = new ExecutionPhase();
        phase.SetAnimationService(animationService);
        phase.Initialize(runner, dispatcher, _ => { });

        runner.Engine.State.Players[0].AssignCardToRegister(0, 0);
        phase.ExecuteRegister();

        events.Should().HaveCount(1);
        events[0].Register.Should().Be(0);
    }

    [Fact]
    public void ExecutionPhase_FiveRegisters_TransitionsToProgrammingPhase()
    {
        var runner = CreateRunnerWithCards(
            new MoveCard { Priority = 100, Spaces = 1 },
            new MoveCard { Priority = 101, Spaces = 1 },
            new MoveCard { Priority = 102, Spaces = 1 },
            new MoveCard { Priority = 103, Spaces = 1 },
            new MoveCard { Priority = 104, Spaces = 1 }
        );
        var dispatcher = new MessageDispatcher();

        var phaseEvents = new List<PhaseChangedEvent>();
        dispatcher.Subscribe<PhaseChangedEvent>(e => phaseEvents.Add(e));

        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, _ => { });

        var player = runner.Engine.State.Players[0];
        for (int i = 0; i < 5; i++)
        {
            player.AssignCardToRegister(i, i);
        }

        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }

        phaseEvents.Should().Contain(e => e.Phase == GamePhaseType.Programming);
    }

    [Fact]
    public void ExecutionPhase_OnEnter_ResetsState()
    {
        var runner = CreateRunnerWithCards(new MoveCard { Priority = 100, Spaces = 1 });
        var dispatcher = new MessageDispatcher();
        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, _ => { });

        runner.Engine.State.CurrentRegister = 3;
        runner.Engine.State.CurrentPlayerIndex = 1;

        phase.OnEnter();

        runner.Engine.State.CurrentRegister.Should().Be(0);
        runner.Engine.State.CurrentPlayerIndex.Should().Be(0);
    }

    [Fact]
    public void ExecutionPhase_ExecuteWithoutCards_DoesNotThrow()
    {
        var runner = CreateRunnerWithCards();
        var dispatcher = new MessageDispatcher();
        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, _ => { });

        var act = () => phase.ExecuteRegister();

        act.Should().NotThrow();
    }

    [Fact]
    public void ExecutionPhase_AnimationService_CalledWhenMoving()
    {
        var runner = CreateRunnerWithCards(new MoveCard { Priority = 100, Spaces = 1 });
        var dispatcher = new MessageDispatcher();
        var animationService = new MockAnimationService();

        var phase = new ExecutionPhase();
        phase.SetAnimationService(animationService);
        phase.Initialize(runner, dispatcher, _ => { });

        var player = runner.Engine.State.Players[0];
        player.Robot.SetDirection(Direction.East);
        player.AssignCardToRegister(0, 0);

        phase.ExecuteRegister();

        animationService.PlayAndWaitCallCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void ExecutionPhase_CurrentRegister_IncrementsSequentially()
    {
        var runner = CreateRunnerWithCards(
            new MoveCard { Priority = 100, Spaces = 1 },
            new MoveCard { Priority = 101, Spaces = 1 },
            new MoveCard { Priority = 102, Spaces = 1 }
        );
        var dispatcher = new MessageDispatcher();
        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, _ => { });

        var player = runner.Engine.State.Players[0];
        for (int i = 0; i < 3; i++)
        {
            player.AssignCardToRegister(i, i);
        }

        runner.Engine.State.CurrentRegister.Should().Be(0);
        phase.ExecuteRegister();
        runner.Engine.State.CurrentRegister.Should().Be(1);
        phase.ExecuteRegister();
        runner.Engine.State.CurrentRegister.Should().Be(2);
    }

    [Fact]
    public void ExecutionPhase_ClearRegisters_AfterCompletion()
    {
        var runner = CreateRunnerWithCards(
            new MoveCard { Priority = 100, Spaces = 1 },
            new MoveCard { Priority = 101, Spaces = 1 },
            new MoveCard { Priority = 102, Spaces = 1 },
            new MoveCard { Priority = 103, Spaces = 1 },
            new MoveCard { Priority = 104, Spaces = 1 }
        );
        var dispatcher = new MessageDispatcher();
        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, _ => { });

        var player = runner.Engine.State.Players[0];
        for (int i = 0; i < 5; i++)
        {
            player.AssignCardToRegister(i, i);
        }

        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }

        player.Registers.Should().AllSatisfy(r => r.Should().BeNull());
    }
}
