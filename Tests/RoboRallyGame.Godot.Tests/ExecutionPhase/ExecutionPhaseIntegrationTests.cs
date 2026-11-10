namespace RoboRallyGame.Godot.Tests.ExecutionPhase;

using System.Collections.Generic;
using RoboRallyGame.Core.Cards;
using RoboRallyGame.Core.Engine;
using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Messages;
using RoboRallyGame.Core.Models;
using RoboRallyGame.Godot.Animations;
using RoboRallyGame.Godot.Phases;
using Xunit;

public class ExecutionPhaseIntegrationTests
{
    private static void BindEngineToDispatcher(GameRunner runner, MessageDispatcher dispatcher)
    {
        var engine = runner.Engine;
        if (engine == null) return;
        
        engine.OnMove += e => dispatcher.Publish(e);
        engine.OnRotate += e => dispatcher.Publish(e);
        engine.OnLaserFire += e => dispatcher.Publish(e);
        engine.OnDamage += e => dispatcher.Publish(e);
        engine.OnConveyorMove += e => dispatcher.Publish(e);
        engine.OnCheckpoint += e => dispatcher.Publish(e);
        engine.OnRobotDestroyed += e => dispatcher.Publish(e);
        engine.OnRobotRebooted += e => dispatcher.Publish(e);
        engine.OnRobotRebooting += e => dispatcher.Publish(e);
        engine.OnRobotRepaired += e => dispatcher.Publish(e);
        engine.OnLifeTokenLost += e => dispatcher.Publish(e);
        engine.OnMovementPhaseStarted += e => dispatcher.Publish(e);
        engine.OnConveyorPhaseStarted += e => dispatcher.Publish(e);
        engine.OnLaserPhaseStarted += e => dispatcher.Publish(e);
        engine.OnRegisterExecuted += e => dispatcher.Publish(e);
        engine.OnPhaseExecutionComplete += e => dispatcher.Publish(e);
        engine.OnAllRegistersComplete += () => dispatcher.Publish(new AllRegistersCompleteEvent());
        engine.OnGameOver += e => dispatcher.Publish(e);
        engine.OnUpgradeAcquired += e => dispatcher.Publish(e);
    }

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
        BindEngineToDispatcher(runner, dispatcher);
        var animationService = new MockAnimationService();

        var events = new List<RegisterExecutedEvent>();
        dispatcher.Subscribe<RegisterExecutedEvent>(e => events.Add(e));

        var phase = new ExecutionPhase();
        phase.SetAnimationService(animationService);
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

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
        BindEngineToDispatcher(runner, dispatcher);

        var phaseEvents = new List<PhaseChangedEvent>();
        dispatcher.Subscribe<PhaseChangedEvent>(e => phaseEvents.Add(e));

        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

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
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        runner.Engine.SetCurrentRegister(3);
        runner.Engine.SetCurrentPlayerIndex(1);

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
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        var act = () => phase.ExecuteRegister();

        act.Should().NotThrow();
    }

    [Fact]
    public void ExecutionPhase_AnimationService_CalledWhenMoving()
    {
        var runner = CreateRunnerWithCards(new MoveCard { Priority = 100, Spaces = 1 });
        var dispatcher = new MessageDispatcher();
        BindEngineToDispatcher(runner, dispatcher);
        var animationService = new MockAnimationService();

        var phase = new ExecutionPhase();
        phase.SetAnimationService(animationService);
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

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
        BindEngineToDispatcher(runner, dispatcher);
        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

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
        BindEngineToDispatcher(runner, dispatcher);
        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        var player = runner.Engine.State.Players[0];
        for (int i = 0; i < 5; i++)
        {
            player.AssignCardToRegister(i, i);
        }

        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }
        
        phase.ExecuteRegister();

        player.Registers.Should().AllSatisfy(r => r.Card.Should().BeNull());
    }

    [Fact]
    public void ExecutionPhase_CompleteAllRegisters_PublishesRegistersClearedEvent()
    {
        var runner = CreateRunnerWithCards(
            new MoveCard { Priority = 100, Spaces = 1 },
            new MoveCard { Priority = 101, Spaces = 1 },
            new MoveCard { Priority = 102, Spaces = 1 },
            new MoveCard { Priority = 103, Spaces = 1 },
            new MoveCard { Priority = 104, Spaces = 1 }
        );
        var dispatcher = new MessageDispatcher();
        BindEngineToDispatcher(runner, dispatcher);
        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        var events = new List<RegistersClearedEvent>();
        dispatcher.Subscribe<RegistersClearedEvent>(e => events.Add(e));

        var player = runner.Engine.State.Players[0];
        for (int i = 0; i < 5; i++)
        {
            player.AssignCardToRegister(i, i);
        }

        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }

        events.Should().HaveCount(1);
    }
}
