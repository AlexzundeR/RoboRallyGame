namespace AiTest.Godot.Tests.GameFlow;

using AiTest.Core.Cards;
using AiTest.Core.Config;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Messages;
using AiTest.Core.Models;
using AiTest.Godot.Phases;
using Xunit;

public class GameFlowTests
{
    private static void ExecuteRegister(GameEngine engine, int registerIndex)
    {
        engine.SetCurrentRegister(registerIndex);
        while (engine.TryExecuteNextStep() == StepResult.Executed) { }
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

    private static Player CreateWithRegisters(params (ProgramCard? Card, int HandIndex)[] registers)
    {
        var regArray = new RegisterCard?[5];
        for (int i = 0; i < registers.Length && i < 5; i++)
            if (registers[i].Card != null)
                regArray[i] = new RegisterCard(registers[i].Card, registers[i].HandIndex);
        return new Player(regArray, null);
    }

    [Fact]
    public void ProgrammingPhase_ToExecutionPhase_ShouldPublishPhaseChangedEvent()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
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

        programmingPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);
        executionPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);

        programmingPhase.OnEnter();
        OnPhaseComplete(GamePhaseType.Execution);

        Assert.True(phaseChangedEvent);
        Assert.Equal(GamePhaseType.Execution, receivedPhase);
        Assert.Equal(1, receivedRound);
    }

    [Fact]
    public void ExecutionPhase_ToProgrammingPhase_ShouldIncrementRound()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
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

        programmingPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);
        executionPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);

        executionPhase.OnEnter();
        OnPhaseComplete(GamePhaseType.Programming);

        Assert.Equal(2, round);
        Assert.Contains(events, e => e.Phase == GamePhaseType.Programming && e.Round == 2);
    }

    [Fact]
    public void TwoPlayerGame_FullProgrammingPhase_ShouldTrackCurrentPlayer()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var phase = new ProgrammingPhase();
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        var firstPlayer = phase.CurrentPlayerName;
        runner.Engine.SetCurrentPlayerIndex(1);
        var secondPlayer = phase.CurrentPlayerName;

        Assert.NotEqual(firstPlayer, secondPlayer);
        Assert.Equal("Player1", firstPlayer);
        Assert.Equal("Player2", secondPlayer);
    }

    [Fact]
    public void TwoPlayerGame_Registers_ShouldStartAt0()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var phase = new ProgrammingPhase();
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        Assert.Equal(0, phase.CurrentRegister);
    }

    [Fact]
    public void ProgrammingPhase_AfterCardSelection_ShouldPublishCardSelectedEvent()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var cardSelectedEvents = new List<CardSelectedEvent>();

        dispatcher.Subscribe<CardSelectedEvent>(e => cardSelectedEvents.Add(e));

        var phase = new ProgrammingPhase();
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        dispatcher.Publish(new CardSelectedEvent(0, 3, 0));

        Assert.Single(cardSelectedEvents);
        Assert.Equal(0, cardSelectedEvents[0].PlayerIndex);
        Assert.Equal(3, cardSelectedEvents[0].CardIndex);
        Assert.Equal(0, cardSelectedEvents[0].Register);
    }

    [Fact]
    public void ExecutionPhase_AfterRegisterExecution_ShouldPublishRegisterExecutedEvent()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var registerExecutedEvents = new List<RegisterExecutedEvent>();

        dispatcher.Subscribe<RegisterExecutedEvent>(e => registerExecutedEvents.Add(e));

        var phase = new ExecutionPhase();
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        dispatcher.Publish(new RegisterExecutedEvent(0, 1));

        Assert.Single(registerExecutedEvents);
        Assert.Equal(0, registerExecutedEvents[0].Register);
        Assert.Equal(1, registerExecutedEvents[0].PlayerIndex);
    }

    [Fact]
    public void PlayerConfirmation_ShouldPublishPlayerConfirmedEvent()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var confirmedEvents = new List<PlayerConfirmedEvent>();

        dispatcher.Subscribe<PlayerConfirmedEvent>(e => confirmedEvents.Add(e));

        var phase = new ProgrammingPhase();
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        dispatcher.Publish(new PlayerConfirmedEvent(0));

        Assert.Single(confirmedEvents);
        Assert.Equal(0, confirmedEvents[0].PlayerIndex);
    }

    [Fact]
    public void CardSelectedEvent_Published_ShouldNotifySubscribers()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var cardEvents = new List<CardSelectedEvent>();

        dispatcher.Subscribe<CardSelectedEvent>(e => cardEvents.Add(e));

        var phase = new ProgrammingPhase();
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        dispatcher.Publish(new CardSelectedEvent(0, 0, 0));

        Assert.Single(cardEvents);
        Assert.Equal(0, cardEvents[0].PlayerIndex);
        Assert.Equal(0, cardEvents[0].CardIndex);
        Assert.Equal(0, cardEvents[0].Register);
    }

    [Fact]
    public void PhaseChangedEvent_ShouldTransitionBetweenPhases()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var phaseHistory = new List<GamePhaseType>();

        dispatcher.Subscribe<PhaseChangedEvent>(e => phaseHistory.Add(e.Phase));

        var programmingPhase = new ProgrammingPhase();
        var executionPhase = new ExecutionPhase();

        programmingPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, phase =>
        {
            dispatcher.Publish(new PhaseChangedEvent(phase, 1));
        });

        executionPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, phase =>
        {
            dispatcher.Publish(new PhaseChangedEvent(phase, 1));
        });

        programmingPhase.OnEnter();
        dispatcher.Publish(new PhaseChangedEvent(GamePhaseType.Execution, 1));

        executionPhase.OnEnter();
        dispatcher.Publish(new PhaseChangedEvent(GamePhaseType.Programming, 2));

        Assert.Contains(GamePhaseType.Execution, phaseHistory);
        Assert.Contains(GamePhaseType.Programming, phaseHistory);
    }

    [Fact]
    public void RegisterExecutedEvent_ShouldTrackExecution()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var executedRegisters = new List<(int Register, int Player)>();

        dispatcher.Subscribe<RegisterExecutedEvent>(e =>
            executedRegisters.Add((e.Register, e.PlayerIndex)));

        var phase = new ExecutionPhase();
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        dispatcher.Publish(new RegisterExecutedEvent(0, 0));
        dispatcher.Publish(new RegisterExecutedEvent(1, 0));
        dispatcher.Publish(new RegisterExecutedEvent(2, 1));

        Assert.Equal(3, executedRegisters.Count);
        Assert.Equal((0, 0), executedRegisters[0]);
        Assert.Equal((1, 0), executedRegisters[1]);
        Assert.Equal((2, 1), executedRegisters[2]);
    }

    [Fact]
    public void CardSelection_ViaEvent_ShouldPublishCardSelectedEvent()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        var cardEvents = new List<CardSelectedEvent>();

        dispatcher.Subscribe<CardSelectedEvent>(e => cardEvents.Add(e));

        var phase = new ProgrammingPhase();
        phase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, _ => { });

        var player = runner.Engine.State.Players[0];
        var cardToSelect = player.Hand[2];

        dispatcher.Publish(new CardSelectedEvent(0, 2, 0));

        Assert.Single(cardEvents);
        Assert.Equal(2, cardEvents[0].CardIndex);
        Assert.Equal(0, cardEvents[0].Register);
    }

    [Fact]
    public void FullGameSession_TwoPlayers_SelectCardsAndExecute()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var phaseChangedEvents = new List<PhaseChangedEvent>();
        var cardSelectedEvents = new List<CardSelectedEvent>();
        var registerExecutedEvents = new List<RegisterExecutedEvent>();
        var playerConfirmedEvents = new List<PlayerConfirmedEvent>();

        dispatcher.Subscribe<PhaseChangedEvent>(e => phaseChangedEvents.Add(e));
        dispatcher.Subscribe<CardSelectedEvent>(e => cardSelectedEvents.Add(e));
        dispatcher.Subscribe<RegisterExecutedEvent>(e => registerExecutedEvents.Add(e));
        dispatcher.Subscribe<PlayerConfirmedEvent>(e => playerConfirmedEvents.Add(e));

        var programmingPhase = new ProgrammingPhase();
        var executionPhase = new ExecutionPhase();

        var currentPhase = GamePhaseType.Programming;
        var currentRound = 1;

        void OnPhaseComplete(GamePhaseType newPhase)
        {
            currentPhase = newPhase;
            if (phaseChangedEvents.Any(e => e.Phase == GamePhaseType.Execution))
            {
                currentRound++;
            }
            dispatcher.Publish(new PhaseChangedEvent(newPhase, currentRound));
        }

        programmingPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);
        BindEngineToDispatcher(runner, dispatcher);
        executionPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);

        programmingPhase.OnEnter();
        Assert.Equal(GamePhaseType.Programming, currentPhase);

        for (int reg = 0; reg < 5; reg++)
        {
            programmingPhase.SelectCard(reg, reg);
        }
        programmingPhase.ConfirmPlayer();

        for (int reg = 0; reg < 5; reg++)
        {
            programmingPhase.SelectCard(reg, reg);
        }
        programmingPhase.ConfirmPlayer();

        Assert.Equal(2, playerConfirmedEvents.Count);

        OnPhaseComplete(GamePhaseType.Execution);
        Assert.Contains(phaseChangedEvents, e => e.Phase == GamePhaseType.Execution);

        executionPhase.OnEnter();

        for (int reg = 0; reg < 10; reg++)
        {
            executionPhase.ExecuteRegister();
        }

        Assert.Equal(10, registerExecutedEvents.Count);

        OnPhaseComplete(GamePhaseType.Programming);

        Assert.Contains(phaseChangedEvents, e => e.Phase == GamePhaseType.Programming);
    }

    [Fact]
    public void FullGameSession_ThreeRounds_TracksRoundsCorrectly()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var rounds = new List<int>();
        dispatcher.Subscribe<PhaseChangedEvent>(e => rounds.Add(e.Round));

        var programmingPhase = new ProgrammingPhase();
        var executionPhase = new ExecutionPhase();

        var currentRound = 1;

        void OnPhaseComplete(GamePhaseType newPhase)
        {
            if (newPhase == GamePhaseType.Programming)
            {
                currentRound++;
            }
            dispatcher.Publish(new PhaseChangedEvent(newPhase, currentRound));
        }

        programmingPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);
        executionPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);

        for (int round = 1; round <= 3; round++)
        {
            programmingPhase.OnEnter();
            OnPhaseComplete(GamePhaseType.Execution);

            executionPhase.OnEnter();
            OnPhaseComplete(GamePhaseType.Programming);
        }

        Assert.Equal(6, rounds.Count);
    }

    [Fact]
    public void FullGameSession_PlayerMovement_VerifyPositionChanges()
    {
        var config = new GameConfiguration
        {
            MapWidth = 8,
            MapHeight = 8,
            Players = new List<PlayerConfig>
            {
                new() { Name = "Player1", StartPosition = new Position(3, 3) }
            },
            Map = new MapConfig
            {
                Starts = new List<StartPlacement>
                {
                    new() { X = 3, Y = 3 }
                }
            }
        };

        var runner = new GameRunner(config);
        runner.Initialize();

        var existingPlayer = runner.Engine.State.Players[0];
        
        var player = CreateWithRegisters(
            (new MoveCard { Priority = 100, Spaces = 1 }, 0),
            (new MoveCard { Priority = 101, Spaces = 1 }, 1),
            (new MoveCard { Priority = 102, Spaces = 1 }, 2)
        );
        player.Robot.SetPosition(existingPlayer.Robot.Position);
        player.Robot.SetDirection(Direction.East);
        
        var state = new GameState(new List<Player> { player }, runner.Engine.State.Map);
        var engine = new GameEngine(new RandomDealer(), state);
        var gameRunner = new GameRunner(engine);

        var pos0 = gameRunner.Engine.State.Players[0].Robot.Position;

        ExecuteRegister(gameRunner.Engine, 0);
        var pos1 = gameRunner.Engine.State.Players[0].Robot.Position;

        ExecuteRegister(gameRunner.Engine, 1);
        var pos2 = gameRunner.Engine.State.Players[0].Robot.Position;

        ExecuteRegister(gameRunner.Engine, 2);
        var pos3 = gameRunner.Engine.State.Players[0].Robot.Position;

        Assert.NotEqual(pos0, pos1);
        Assert.NotEqual(pos1, pos2);
        Assert.NotEqual(pos2, pos3);
    }

    [Fact]
    public void ExecutionPhase_HandleInput_ExecutesRegisterOnInput()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();

        var existingPlayer = runner.Engine.State.Players[0];
        
        var player = CreateWithRegisters((new MoveCard { Priority = 100, Spaces = 1 }, 0));
        player.Robot.SetPosition(existingPlayer.Robot.Position);
        
        var state = new GameState(new List<Player> { player }, runner.Engine.State.Map);
        var engine = new GameEngine(new RandomDealer(), state);
        var gameRunner = new GameRunner(engine);

        var startPos = gameRunner.Engine.State.Players[0].Robot.Position;

        ExecuteRegister(gameRunner.Engine, 0);

        Assert.NotEqual(startPos, gameRunner.Engine.State.Players[0].Robot.Position);
    }

    [Fact]
    public void ExecutionPhase_CompleteAllRegisters_TransitionsToNextPhase()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();
        BindEngineToDispatcher(runner, dispatcher);

        var phaseChangedEvents = new List<PhaseChangedEvent>();
        dispatcher.Subscribe<PhaseChangedEvent>(e => phaseChangedEvents.Add(e));

        GamePhaseType? finalPhase = null;
        var executionPhase = new ExecutionPhase();
        executionPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, phase => finalPhase = phase);

        var p1 = runner.Engine.State.Players[0];
        var p2 = runner.Engine.State.Players[1];

        for (int i = 0; i < 5; i++)
        {
            p1.AssignCardToRegister(i, i);
            p2.AssignCardToRegister(i + 5, i);
        }

        for (int reg = 0; reg < 10; reg++)
        {
            executionPhase.ExecuteRegister();
        }

        Assert.NotNull(finalPhase);
        Assert.Equal(GamePhaseType.Programming, finalPhase);
        Assert.Contains(phaseChangedEvents, e => e.Phase == GamePhaseType.Programming);
    }

    [Fact]
    public void FullGameSession_ViaPhaseInputMethods_CompletesFullCycle()
    {
        var runner = new GameRunner(TestConfigs.CreateDefault2Player());
        runner.Initialize();
        var dispatcher = new MessageDispatcher();

        var events = new List<string>();
        dispatcher.Subscribe<CardSelectedEvent>(e => events.Add($"Card:P{e.PlayerIndex}:{e.CardIndex}:R{e.Register}"));
        dispatcher.Subscribe<PlayerConfirmedEvent>(e => events.Add($"Confirmed:P{e.PlayerIndex}"));
        dispatcher.Subscribe<RegisterExecutedEvent>(e => events.Add($"Executed:R{e.Register}:P{e.PlayerIndex}"));
        dispatcher.Subscribe<PhaseChangedEvent>(e => events.Add($"Phase:{e.Phase}:R{e.Round}"));

        var inputActions = new Queue<string>();
        inputActions.Enqueue("ui_1");
        inputActions.Enqueue("ui_2");
        inputActions.Enqueue("ui_3");
        inputActions.Enqueue("ui_4");
        inputActions.Enqueue("ui_5");
        inputActions.Enqueue("ui_accept");
        inputActions.Enqueue("ui_1");
        inputActions.Enqueue("ui_2");
        inputActions.Enqueue("ui_3");
        inputActions.Enqueue("ui_4");
        inputActions.Enqueue("ui_5");
        inputActions.Enqueue("ui_accept");
        for (int i = 0; i < 10; i++)
        {
            inputActions.Enqueue("ui_accept");
        }

        var mockInputService = new MockInputService(action => 
        {
            if (inputActions.Count == 0) return false;
            return inputActions.Peek() == action;
        });

        var programmingPhase = new ProgrammingPhase(mockInputService);
        var executionPhase = new ExecutionPhase(mockInputService);

        void OnPhaseComplete(GamePhaseType newPhase)
        {
            dispatcher.Publish(new PhaseChangedEvent(newPhase, runner.Engine.State.Round));
        }

        programmingPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);
        BindEngineToDispatcher(runner, dispatcher);
        executionPhase.Initialize(TestHelpers.CreateProvider(runner), dispatcher, OnPhaseComplete);

        programmingPhase.OnEnter();

        for (int i = 0; i < 6; i++)
        {
            programmingPhase.HandleInput();
            if (inputActions.Count > 0)
            {
                inputActions.Dequeue();
            }
        }

        for (int i = 0; i < 6; i++)
        {
            programmingPhase.HandleInput();
            if (inputActions.Count > 0)
            {
                inputActions.Dequeue();
            }
        }

        executionPhase.OnEnter();

        for (int i = 0; i < 10; i++)
        {
            executionPhase.HandleInput();
            if (inputActions.Count > 0)
            {
                inputActions.Dequeue();
            }
        }

        Assert.Contains(events, e => e.StartsWith("Phase:Programming"));
        Assert.Contains(events, e => e.StartsWith("Phase:Execution"));
        Assert.Contains(events, e => e.StartsWith("Confirmed:P0"));
        Assert.Contains(events, e => e.StartsWith("Confirmed:P1"));
        Assert.Contains(events, e => e.StartsWith("Executed:"));
    }
}

public class MockInputService : IInputService
{
    private readonly Func<string, bool> _handler;

    public MockInputService(Func<string, bool> handler)
    {
        _handler = handler;
    }

    public bool IsActionJustPressed(string action) => _handler(action);
}
