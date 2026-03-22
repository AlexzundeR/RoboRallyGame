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
        
        phase.Initialize(runner, dispatcher, _ => { });
        
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