namespace AiTest.Core.Tests.Engine;

using AiTest.Core.Cards;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Core.Upgrades;

public class GameEngineTests
{
    private static readonly IReadOnlyList<ProgramCard> DefaultHand = new List<ProgramCard>
    {
        new MoveCard { Priority = 100, Spaces = 1 },
        new MoveCard { Priority = 101, Spaces = 2 },
        new MoveCard { Priority = 102, Spaces = 3 },
        new TurnCard { Priority = 150, IsLeft = true },
        new TurnCard { Priority = 151, IsLeft = false },
        new UTurnCard { Priority = 350 },
        new MoveCard { Priority = 103, Spaces = 1 },
        new MoveCard { Priority = 104, Spaces = 2 },
        new MoveCard { Priority = 105, Spaces = 3 },
    };

    private static void ExecuteRegister(GameEngine engine, int registerIndex)
    {
        engine.BeginRegisterExecution(registerIndex);
        while (engine.TryExecuteNextStep() == StepResult.Executed) { }
    }

    private static void ExecuteFullRegister(GameEngine engine) => ExecuteRegister(engine, 0);

    private static Player CreateWithRegisters(params (ProgramCard Card, int HandIndex)[] registers)
    {
        var regArray = new RegisterCard?[5];
        for (int i = 0; i < registers.Length && i < 5; i++)
            regArray[i] = new RegisterCard(registers[i].Card, registers[i].HandIndex);
        return new Player(regArray, null);
    }

    private static Player CreatePlayerWithRobot(int robotId, Position pos, Direction dir, params (ProgramCard Card, int HandIndex)[] registers)
    {
        var regArray = new RegisterCard?[5];
        for (int i = 0; i < registers.Length && i < 5; i++)
            regArray[i] = new RegisterCard(registers[i].Card, registers[i].HandIndex);
        var robot = new Robot(robotId, pos, dir);
        var player = new Player(regArray, null) { Robot = robot };
        return player;
    }

    private GameEngine CreateEngine(GameState state)
    {
        var engine = new GameEngine(new RandomDealer(), state);
        var map = new GameMap(10, 10);
        map.SetTile(1, 1, TileFactory.Start());
        map.SetTile(8, 1, TileFactory.Start());
        map.SetTile(1, 8, TileFactory.Start());
        map.SetTile(8, 8, TileFactory.Start());
        map.SetTile(5, 5, TileFactory.Checkpoint(1));
        engine.Initialize(map);
        return engine;
    }
    
    private GameEngine CreateEngine()
    {
        return CreateEngine(new GameState());
    }

    private GameEngine CreateEngineWithPlayers(int playerCount)
    {
        var hands = Enumerable.Range(0, playerCount)
            .Select(_ => DefaultHand)
            .ToArray();
        var dealer = new FixedDealer(hands);
        var engine = new GameEngine(dealer);
        var map = new GameMap(10, 10);
        for (int i = 0; i < playerCount; i++)
        {
            map.SetTile(1 + i * 2, 1, TileFactory.Start());
        }
        map.SetTile(5, 5, TileFactory.Checkpoint(1));
        engine.Initialize(map);
        for (int i = 0; i < playerCount; i++)
        {
            engine.AddPlayer($"Player{i + 1}");
        }
        return engine;
    }

    [Fact]
    public void Initialize_ShouldSetMapAndPhase()
    {
        var engine = new GameEngine();
        var map = new GameMap(10, 10, "Test");
        
        engine.Initialize(map);
        
        Assert.Equal(map, engine.State.Map);
        Assert.Equal(GamePhase.Programming, engine.State.Phase);
    }

    [Fact]
    public void AddPlayer_ShouldCreatePlayerWithRobot()
    {
        var engine = CreateEngine();
        
        engine.AddPlayer("TestPlayer");
        
        Assert.Single(engine.State.Players);
        var player = engine.State.Players[0];
        Assert.Equal("TestPlayer", player.Name);
        Assert.Equal(1, player.Id);
        Assert.NotNull(player.Robot);
    }

    [Fact]
    public void StartGame_ShouldSetPhaseAndRound()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        
        engine.StartGame();
        
        Assert.Equal(GamePhase.Programming, engine.State.Phase);
        Assert.Equal(1, engine.State.Round);
    }

    [Fact]
    public void StartGame_ShouldDealCardsToPlayers()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        
        engine.StartGame();
        
        Assert.Equal(9, engine.State.Players[0].AvailableCardCount);
    }

    [Fact]
    public void ExecuteRegister_MoveCard_ShouldMoveRobot()
    {
        var moveCard = new MoveCard { Priority = 100, Spaces = 1 };
        var player = CreateWithRegisters((moveCard, 0));
        player.Robot.SetPosition(new Position(1, 1));
        
        var engine = CreateEngine(new GameState(new List<Player> { player }));
        
        var startPos = player.Robot.Position;
        ExecuteFullRegister(engine);
        
        Assert.NotEqual(startPos, player.Robot.Position);
    }

    [Fact]
    public void ExecuteRegister_TurnCard_ShouldRotateRobot()
    {
        var turnCard = new TurnCard { Priority = 150, IsLeft = true };
        var player = CreateWithRegisters((turnCard, 0));
        player.Robot.SetPosition(new Position(1, 1));
        player.Robot.SetDirection(Direction.North);
        
        var engine = CreateEngine(new GameState(new List<Player> { player }));
        
        ExecuteFullRegister(engine);
        
        Assert.Equal(Direction.West, player.Robot.Direction);
    }

    [Fact]
    public void ExecuteRegister_BackUpCard_ShouldMoveBackward()
    {
        var backUpCard = new BackUpCard { Priority = 500 };
        var player = CreateWithRegisters((backUpCard, 0));
        player.Robot.SetPosition(new Position(5, 5));
        player.Robot.SetDirection(Direction.North);
        
        var engine = CreateEngine(new GameState(new List<Player> { player }));
        
        ExecuteFullRegister(engine);
        
        Assert.Equal(new Position(5, 6), player.Robot.Position);
    }

    [Fact]
    public void ProcessBoardElements_Gear_ShouldRotateRobot()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(3, 3, TileFactory.Gear(true));
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.SetPosition(new Position(3, 3));
        player.Robot.SetDirection(Direction.North);
        
        ExecuteRegister(engine, 0);
        
        Assert.Equal(Direction.East, player.Robot.Direction);
    }

    [Fact]
    public void ProcessBoardElements_Checkpoint_ShouldAdvanceCheckpoint()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.SetPosition(new Position(5, 5));
        
        ExecuteRegister(engine, 0);
        
        Assert.Equal(1, player.Robot.CheckpointsReached);
    }

    [Fact]
    public void ProcessBoardElements_Pit_ShouldTriggerReboot()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(3, 3, TileFactory.Pit());
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.SetPosition(new Position(3, 3));
        
        ExecuteRegister(engine, 0);
        
        Assert.Equal(RobotStatus.Active, player.Robot.Status);
        Assert.Equal(2, player.Robot.Damage);
    }

    [Fact]
    public void ProcessBoardElements_Pit_ShouldRespawnAtLastCheckpoint()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(5, 5, TileFactory.Checkpoint(1));
        engine.State.Map.SetTile(7, 7, TileFactory.Checkpoint(2));
        engine.State.Map.SetTile(3, 3, TileFactory.Pit());
        
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        
        player.Robot.SetPosition(new Position(5, 5));
        ExecuteRegister(engine, 0);
        
        player.Robot.SetPosition(new Position(3, 3));
        ExecuteRegister(engine, 0);
        
        Assert.Equal(RobotStatus.Active, player.Robot.Status);
        Assert.Equal(new Position(5, 5), player.Robot.Position);
    }

    [Fact]
    public void ExecuteMovementStep_PushRobot_ShouldPushRobot()
    {
        var p1 = CreatePlayerWithRobot(1, new Position(3, 3), Direction.East, (new MoveCard { Priority = 100, Spaces = 1 }, 0));
        var p2 = CreatePlayerWithRobot(2, new Position(4, 3), Direction.North, (new MoveCard { Priority = 101, Spaces = 1 }, 0));
        
        var map = new GameMap(10, 10);
        map.SetTile(1, 1, TileFactory.Start());
        map.SetTile(3, 3, TileFactory.Floor());
        map.SetTile(4, 3, TileFactory.Floor());
        map.SetTile(5, 3, TileFactory.Floor());
        
        var engine = new GameEngine(new RandomDealer(), new GameState(new List<Player> { p1, p2 }));
        engine.Initialize(map);
        
        engine.BeginRegisterExecution(0);
        
        while (engine.TryExecuteNextStep() == StepResult.Executed) { }
        
        Assert.Equal(new Position(4, 3), p1.Robot.Position);
        Assert.Equal(new Position(4, 2), p2.Robot.Position);
    }

    [Fact]
    public void IsGameOver_WhenNoCheckpoints_ShouldReturnFalse()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        
        Assert.False(engine.IsGameOver());
    }

    [Fact]
    public void IsGameOver_WhenAllCheckpointsReached_ShouldReturnTrue()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.SetCheckpointsReached(4);
        
        Assert.True(engine.IsGameOver());
    }

    [Fact]
    public void GetWinner_WhenPlayerWins_ShouldReturnPlayer()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.SetCheckpointsReached(4);
        
        var winner = engine.GetWinner();
        
        Assert.Equal(player, winner);
    }

    [Fact]
    public void NextRound_ShouldIncrementRoundAndClearRegisters()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        engine.StartGame();
        
        engine.NextRound();
        
        Assert.Equal(2, engine.State.Round);
        Assert.Null(engine.State.Players[0].GetRegister(0));
    }

    [Fact]
    public void FullRound_PlayerMoveTurnAndCheckpoint_ShouldAdvanceGameState()
    {
        var player = CreateWithRegisters(
            (new MoveCard { Priority = 100, Spaces = 3 }, 0),
            (new TurnCard { Priority = 150, IsLeft = true }, 1),
            (new MoveCard { Priority = 101, Spaces = 2 }, 2)
        );
        player.Robot.SetPosition(new Position(2, 2));
        player.Robot.SetDirection(Direction.East);
        
        var engine = CreateEngine(new GameState(new List<Player> { player }));
        
        ExecuteFullRegister(engine);
        
        Assert.Equal(new Position(5, 2), player.Robot.Position);
        
        ExecuteRegister(engine, 1);
        
        Assert.Equal(Direction.North, player.Robot.Direction);
        
        player.Robot.SetPosition(new Position(5, 5));
        ExecuteRegister(engine, 1);
        
        Assert.Equal(1, player.Robot.CheckpointsReached);
    }

    [Fact]
    public void FullRound_TwoPlayersCollision_ShouldResolvePushing()
    {
        var p1 = CreatePlayerWithRobot(1, new Position(3, 1), Direction.East, (new MoveCard { Priority = 100, Spaces = 2 }, 0));
        var p2 = CreatePlayerWithRobot(2, new Position(5, 1), Direction.West, (new MoveCard { Priority = 101, Spaces = 2 }, 0));
        
        var engine = CreateEngine(new GameState(new List<Player> { p1, p2 }));
        
        ExecuteFullRegister(engine);
        
        Assert.True(p1.Robot.IsAlive);
        Assert.True(p2.Robot.IsAlive);
    }

    [Fact]
    public void FullRound_LaserFiring_ShouldDamageRobots()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        engine.AddPlayer("Player2");
        
        var p1 = engine.State.Players[0];
        var p2 = engine.State.Players[1];
        
        p1.Robot.SetPosition(new Position(2, 2));
        p1.Robot.SetDirection(Direction.East);
        p2.Robot.SetPosition(new Position(4, 2));
        p2.Robot.SetDirection(Direction.North);
        
        ExecuteFullRegister(engine);
        
        Assert.Equal(1, p2.Robot.Damage);
    }

    [Fact]
    public void FullRound_PlayerWinsAfter4Checkpoints_ShouldEndGame()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        
        var player = engine.State.Players[0];
        player.Robot.SetCheckpointsReached(4);
        
        Assert.True(engine.IsGameOver());
        Assert.Equal(player, engine.GetWinner());
    }

    [Fact]
    public void DealCards_WithDamage_ShouldDealFewerCards()
    {
        var dealer = new RandomDealer();
        var deck = CreateDefaultDeck();
        
        var robot = new Robot { Id = 1 };
        robot.AddDamage(5);
        var player = new Player { Robot = robot };
        
        dealer.Deal(new[] { player }, deck);
        
        Assert.Equal(4, player.AvailableCardCount);
    }

    [Fact]
    public void DealCards_With9Damage_ShouldDealNoCards()
    {
        var dealer = new RandomDealer();
        var deck = CreateDefaultDeck();
        
        var robot = new Robot { Id = 1 };
        robot.AddDamage(9);
        var player = new Player { Robot = robot };
        
        dealer.Deal(new[] { player }, deck);
        
        Assert.Equal(0, player.AvailableCardCount);
    }

    [Fact]
    public void DealCards_With0Damage_ShouldDeal9Cards()
    {
        var dealer = new RandomDealer();
        var deck = CreateDefaultDeck();
        
        var player = new Player();
        
        dealer.Deal(new[] { player }, deck);
        
        Assert.Equal(9, player.AvailableCardCount);
    }

    [Fact]
    public void NextRound_PoweredDownRobot_ShouldWakeUpAndNotReturnOldCardsToHand()
    {
        var robot = new Robot { Id = 1 };
        robot.SetStatus(RobotStatus.PowerDown);
        
        var hand = new ProgramCard?[9];
        hand[0] = new MoveCard { Priority = 100, Spaces = 1 };
        var player = new Player(null, hand) { Robot = robot };
        
        var map = new GameMap(10, 10);
        var state = new GameState(new List<Player> { player }, map);
        var engine = CreateEngine(state);
        
        engine.NextRound();
        
        Assert.Equal(RobotStatus.Active, robot.Status);
        Assert.Equal(9, player.AvailableCardCount);
        Assert.NotEqual(1, player.AvailableCardCount);
    }

    [Fact]
    public void NextRound_ActiveRobot_ShouldReturnCardsToHand()
    {
        var robot = new Robot { Id = 1 };
        var hand = new ProgramCard?[9];
        hand[0] = new MoveCard { Priority = 100, Spaces = 1 };
        var player = new Player(null, hand) { Robot = robot };
        
        var map = new GameMap(10, 10);
        var state = new GameState(new List<Player> { player }, map);
        var engine = CreateEngine(state);
        
        engine.NextRound();
        
        Assert.Equal(RobotStatus.Active, robot.Status);
        Assert.Equal(9, player.AvailableCardCount);
    }

    [Fact]
    public void NextRound_PoweredDownRobot_ShouldRemoveAllDamage()
    {
        var robot = new Robot { Id = 1 };
        robot.AddDamage(5);
        robot.SetStatus(RobotStatus.PowerDown);
        
        var player = new Player { Robot = robot };
        var map = new GameMap(10, 10);
        var state = new GameState(new List<Player> { player }, map);
        var engine = CreateEngine(state);
        
        engine.NextRound();
        
        Assert.Equal(0, robot.Damage);
    }

    [Fact]
    public void NextRound_RobotOnRepairSite_ShouldRepair1Damage()
    {
        var robot = new Robot { Id = 1 };
        robot.SetPosition(new Position(2, 2));
        
        var player = new Player { Robot = robot };
        var map = new GameMap(10, 10);
        map.SetTile(2, 2, TileFactory.RepairSite());
        
        var repairTile = (AiTest.Core.Models.Components.RepairSiteBehaviorComponent?)map.GetTile(2, 2).GetComponent<AiTest.Core.Models.Components.IEndOfRoundComponent>();
        Assert.NotNull(repairTile);
        
        robot.TakeDamage(3);
        
        var state = new GameState(new List<Player> { player }, map);
        var engine = new GameEngine(new RandomDealer(), state);
        
        engine.NextRound();
        
        Assert.Equal(2, robot.Damage);
    }

    [Fact]
    public void NextRound_RobotOnRepairSiteWithNoDamage_ShouldNotRepair()
    {
        var robot = new Robot { Id = 1 };
        robot.SetPosition(new Position(2, 2));
        
        var player = new Player { Robot = robot };
        var map = new GameMap(10, 10);
        map.SetTile(2, 2, TileFactory.RepairSite());
        var state = new GameState(new List<Player> { player }, map);
        var engine = CreateEngine(state);
        
        engine.NextRound();
        
        Assert.Equal(0, robot.Damage);
    }

    private static List<ProgramCard> CreateDefaultDeck()
    {
        var deck = new List<ProgramCard>();
        for (int i = 0; i < 18; i++)
            deck.Add(new MoveCard { Priority = 100 + i, Spaces = 1 });
        for (int i = 0; i < 12; i++)
            deck.Add(new MoveCard { Priority = 200 + i, Spaces = 2 });
        for (int i = 0; i < 6; i++)
            deck.Add(new MoveCard { Priority = 300 + i, Spaces = 3 });
        for (int i = 0; i < 18; i++)
            deck.Add(new TurnCard { Priority = 50 + i * 10, IsLeft = true });
        for (int i = 0; i < 18; i++)
            deck.Add(new TurnCard { Priority = 60 + i * 10, IsLeft = false });
        for (int i = 0; i < 6; i++)
            deck.Add(new UTurnCard { Priority = 400 + i });
        for (int i = 0; i < 6; i++)
            deck.Add(new BackUpCard { Priority = 500 + i });
        return deck;
    }
}
