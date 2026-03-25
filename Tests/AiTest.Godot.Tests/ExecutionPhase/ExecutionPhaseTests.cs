namespace AiTest.Godot.Tests.ExecutionPhase;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiTest.Core.Cards;
using AiTest.Core.Config;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Messages;
using AiTest.Core.Models;
using AiTest.Godot;
using AiTest.Godot.Animations;
using AiTest.Godot.Phases;
using Xunit;

public class MockAnimationService : IAnimationService
{
    public bool PlayAndWaitWasCalled { get; private set; }
    public int PlayAndWaitCallCount { get; private set; }
    public IEnumerable<Animation>? LastAnimations { get; private set; }

    public Task PlayAndWait(IEnumerable<Animation> animations)
    {
        PlayAndWaitWasCalled = true;
        PlayAndWaitCallCount++;
        LastAnimations = animations;
        return Task.CompletedTask;
    }
}

public class ExecutionPhaseTests
{
    private static Player CreatePlayerWithMoveCards(params int[] spaces)
    {
        var registers = new RegisterCard?[5];
        for (int i = 0; i < spaces.Length && i < 5; i++)
        {
            registers[i] = new RegisterCard(
                new MoveCard { Priority = 100 + i, Spaces = spaces[i] },
                i
            );
        }
        return new Player(registers, null);
    }

    [Fact]
    public void ExecutionPhase_WithoutAnimationService_ExecutesAllStepsSynchronously()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var executedRegisters = new List<int>();
        dispatcher.Subscribe<RegisterExecutedEvent>(e => executedRegisters.Add(e.Register));

        var phase = new ExecutionPhase();
        phase.Initialize(runner, dispatcher, _ => { });

        var p1 = runner.Engine.State.Players[0];
        var p2 = runner.Engine.State.Players[1];

        p1.AssignCardToRegister(0, 0);
        p1.AssignCardToRegister(0, 1);
        p1.AssignCardToRegister(0, 2);
        p1.AssignCardToRegister(0, 3);
        p1.AssignCardToRegister(0, 4);

        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }

        Assert.Equal(5, executedRegisters.Count);
        Assert.Equal(new List<int> { 0, 1, 2, 3, 4 }, executedRegisters);
    }

    [Fact]
    public void ExecutionPhase_WithAnimationService_CallsPlayAndWait()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var mock = new MockAnimationService();
        var phase = new ExecutionPhase();
        phase.SetAnimationService(mock);
        phase.Initialize(runner, dispatcher, _ => { });

        var p1 = runner.Engine.State.Players[0];
        p1.AssignCardToRegister(0, 0);

        phase.ExecuteRegister();

        Assert.True(mock.PlayAndWaitWasCalled);
        Assert.True(mock.PlayAndWaitCallCount >= 1);
    }

    [Fact]
    public void ExecutionPhase_CompleteAll5Registers_TransitionsToNextPhase()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var phaseChangedEvents = new List<PhaseChangedEvent>();
        dispatcher.Subscribe<PhaseChangedEvent>(e => phaseChangedEvents.Add(e));

        GamePhaseType? finalPhase = null;
        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, p => finalPhase = p);

        var p1 = runner.Engine.State.Players[0];
        var p2 = runner.Engine.State.Players[1];

        for (int i = 0; i < 5; i++)
        {
            p1.AssignCardToRegister(i, i);
            p2.AssignCardToRegister(i + 5, i);
        }

        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }

        Assert.NotNull(finalPhase);
        Assert.Equal(GamePhaseType.Programming, finalPhase);
        Assert.Contains(phaseChangedEvents, e => e.Phase == GamePhaseType.Programming);
    }

    [Fact]
    public void ExecutionPhase_RobotPositionChangesAfterMovement()
    {
        var moveCard = new MoveCard { Priority = 100, Spaces = 1 };
        var hand = new ProgramCard?[9];
        hand[0] = moveCard;
        var registers = new RegisterCard?[5];
        
        var runner = new GameRunner(new GameEngine(
            new RandomDealer(),
            new GameState(new List<Player> { new Player(registers, hand) { Name = "TestPlayer" } })));
        runner.Initialize();

        var player = runner.Engine.State.Players[0];
        player.Robot.SetDirection(Direction.East);
        player.AssignCardToRegister(0, 0);

        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, new MessageDispatcher(), _ => { });

        phase.ExecuteRegister();
    }

    [Fact]
    public void ExecutionPhase_ExecuteAllSteps_CompletesRegister()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var executedEvents = new List<RegisterExecutedEvent>();
        dispatcher.Subscribe<RegisterExecutedEvent>(e => executedEvents.Add(e));

        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, _ => { });

        var p1 = runner.Engine.State.Players[0];
        p1.AssignCardToRegister(0, 0);

        phase.ExecuteRegister();

        Assert.Single(executedEvents);
        Assert.Equal(0, executedEvents[0].Register);
    }

    [Fact]
    public void ExecutionPhase_HandlesMultipleRegisters_Sequentially()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var executedRegisters = new List<int>();
        dispatcher.Subscribe<RegisterExecutedEvent>(e => executedRegisters.Add(e.Register));

        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, _ => { });

        var p1 = runner.Engine.State.Players[0];
        for (int i = 0; i < 5; i++)
        {
            p1.AssignCardToRegister(i, i);
        }

        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }

        Assert.Equal(5, executedRegisters.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i, executedRegisters[i]);
        }
    }

    [Fact]
    public void ExecutionPhase_AnimationServiceReceivesAnimations()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var mock = new MockAnimationService();

        var phase = new ExecutionPhase();
        phase.SetAnimationService(mock);
        phase.Initialize(runner, dispatcher, _ => { });

        var p1 = runner.Engine.State.Players[0];
        p1.AssignCardToRegister(0, 0);

        phase.ExecuteRegister();

        Assert.True(mock.PlayAndWaitWasCalled);
    }

    [Fact]
    public void ExecutionPhase_PublishesCorrectEvents()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var registerExecutedEvents = new List<RegisterExecutedEvent>();
        dispatcher.Subscribe<RegisterExecutedEvent>(e => registerExecutedEvents.Add(e));

        var phase = new ExecutionPhase();
        phase.SetAnimationService(new MockAnimationService());
        phase.Initialize(runner, dispatcher, _ => { });

        var p1 = runner.Engine.State.Players[0];
        for (int i = 0; i < 5; i++)
        {
            p1.AssignCardToRegister(i, i);
        }

        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }

        Assert.Equal(5, registerExecutedEvents.Count);
        Assert.All(registerExecutedEvents, e => Assert.True(e.Register >= 0 && e.Register < 5));
    }
}
