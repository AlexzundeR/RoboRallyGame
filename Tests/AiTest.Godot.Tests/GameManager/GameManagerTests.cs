namespace AiTest.Godot.Tests.GameManager;

using AiTest.Core.Cards;
using AiTest.Core.Entities;
using AiTest.Core.Engine;
using AiTest.Core.Messages;
using AiTest.Core.Config;
using AiTest.Godot.Phases;
using FluentAssertions;

public class GameManagerTests
{
    [Fact]
    public void ExecutionPhase_Complete_ClearsRegisters()
    {
        var hand = new ProgramCard?[9];
        hand[0] = new MoveCard { Priority = 100, Spaces = 1 };
        hand[1] = new MoveCard { Priority = 101, Spaces = 1 };
        
        var player = new Player(null, hand);
        
        player.AssignCardToRegister(0, 0);
        player.AssignCardToRegister(1, 1);
        
        player.Registers[0].Should().NotBeNull();
        player.Registers[1].Should().NotBeNull();
        
        player.ClearRegisters();
        
        player.Registers.Should().AllSatisfy(r => r.Should().BeNull());
    }

    [Fact]
    public void RegistersClearedEvent_ShouldBePublished_OnPhaseComplete()
    {
        var player = CreatePlayerWithRegisters();
        var engine = new GameEngine(new RandomDealer(), new GameState(new List<Player> { player }));
        var dispatcher = new MessageDispatcher();
        var phase = new ExecutionPhase();
        
        var events = new List<RegistersClearedEvent>();
        dispatcher.Subscribe<RegistersClearedEvent>(e => events.Add(e));
        
        var runner = new GameRunner(engine);
        runner.Initialize();
        
        BindEngineToDispatcher(runner, dispatcher);
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });
        
        var gamePlayer = runner.Engine.State.Players[0];
        for (int i = 0; i < 5; i++)
        {
            gamePlayer.AssignCardToRegister(i, i);
        }
        
        for (int i = 0; i < 5; i++)
        {
            phase.ExecuteRegister();
        }
        phase.ExecuteRegister();
        
        events.Should().HaveCount(1);
    }

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
        engine.OnRobotReboot += e => dispatcher.Publish(e);
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

    [Fact]
    public void ClearRegisters_AfterExecution_MakesRegistersNull()
    {
        var player = CreatePlayerWithCards(
            new MoveCard { Priority = 100, Spaces = 1 },
            new MoveCard { Priority = 101, Spaces = 1 },
            new MoveCard { Priority = 102, Spaces = 1 },
            new MoveCard { Priority = 103, Spaces = 1 },
            new MoveCard { Priority = 104, Spaces = 1 }
        );
        
        for (int i = 0; i < 5; i++)
        {
            player.AssignCardToRegister(i, i);
        }
        
        player.Registers[0].Should().NotBeNull();
        player.Registers[4].Should().NotBeNull();
        
        player.ClearRegisters();
        
        player.Registers.Should().AllSatisfy(r => r.Should().BeNull());
    }

    private static Player CreatePlayerWithRegisters()
    {
        var registers = new RegisterCard?[5];
        var hand = new ProgramCard?[9];
        return new Player(registers, hand);
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
}