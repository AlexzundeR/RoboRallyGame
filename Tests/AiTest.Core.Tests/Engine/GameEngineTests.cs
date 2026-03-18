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

    private GameEngine CreateEngine()
    {
        var dealer = new FixedDealer(DefaultHand);
        var engine = new GameEngine(dealer);
        var map = new GameMap(10, 10);
        map.SetTile(1, 1, Tile.Start);
        map.SetTile(8, 1, Tile.Start);
        map.SetTile(1, 8, Tile.Start);
        map.SetTile(8, 8, Tile.Start);
        map.SetTile(5, 5, Tile.Checkpoint(1));
        engine.Initialize(map);
        return engine;
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
            map.SetTile(1 + i * 2, 1, Tile.Start);
        }
        map.SetTile(5, 5, Tile.Checkpoint(1));
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
        var engine = CreateEngine();
        var moveCard = new MoveCard { Priority = 100, Spaces = 1 };
        var player = Player.CreateWithRegisters((moveCard, 0));
        player.Robot.SetPosition(new Position(1, 1));
        engine.State.Players.Add(player);
        
        var startPos = player.Robot.Position;
        engine.ExecuteRegister(0);
        
        Assert.NotEqual(startPos, player.Robot.Position);
    }

    [Fact]
    public void ExecuteRegister_TurnCard_ShouldRotateRobot()
    {
        var engine = CreateEngine();
        var turnCard = new TurnCard { Priority = 150, IsLeft = true };
        var player = Player.CreateWithRegisters((turnCard, 0));
        player.Robot.SetPosition(new Position(1, 1));
        engine.State.Players.Add(player);
        player.Robot.SetDirection(Direction.North);
        
        engine.ExecuteRegister(0);
        
        Assert.Equal(Direction.West, player.Robot.Direction);
    }

    [Fact]
    public void ExecuteRegister_BackUpCard_ShouldMoveBackward()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        engine.StartGame();
        
        var player = engine.State.Players[0];
        player.Robot.SetPosition(new Position(5, 5));
        player.Robot.SetDirection(Direction.North);
        var backUpCard = new BackUpCard { Priority = 500 };
        player.Registers[0] = new RegisterCard(backUpCard, 0);
        
        engine.ExecuteRegister(0);
        
        Assert.Equal(new Position(5, 6), player.Robot.Position);
    }

    [Fact]
    public void ProcessBoardElements_Gear_ShouldRotateRobot()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(3, 3, Tile.Gear(true));
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.SetPosition(new Position(3, 3));
        player.Robot.SetDirection(Direction.North);
        
        engine.ProcessBoardElements();
        
        Assert.Equal(Direction.East, player.Robot.Direction);
    }

    [Fact]
    public void ProcessBoardElements_Checkpoint_ShouldAdvanceCheckpoint()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.SetPosition(new Position(5, 5));
        
        engine.ProcessBoardElements();
        
        Assert.Equal(1, player.Robot.CheckpointsReached);
    }

    [Fact]
    public void ProcessBoardElements_Pit_ShouldTriggerReboot()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(3, 3, Tile.Pit);
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.SetPosition(new Position(3, 3));
        
        engine.ProcessBoardElements();
        
        Assert.Equal(RobotStatus.Rebooting, player.Robot.Status);
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
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        
        var player = engine.State.Players[0];
        player.Robot.SetPosition(new Position(2, 2));
        player.Robot.SetDirection(Direction.East);
        
        player.Registers[0] = new RegisterCard(new MoveCard { Priority = 100, Spaces = 3 }, 0);
        player.Registers[1] = new RegisterCard(new TurnCard { Priority = 150, IsLeft = true }, 1);
        player.Registers[2] = new RegisterCard(new MoveCard { Priority = 101, Spaces = 2 }, 2);
        
        engine.ExecuteRegister(0);
        
        Assert.Equal(new Position(5, 2), player.Robot.Position);
        
        engine.ExecuteRegister(1);
        
        Assert.Equal(Direction.North, player.Robot.Direction);
        
        player.Robot.SetPosition(new Position(5, 5));
        engine.ProcessBoardElements();
        
        Assert.Equal(1, player.Robot.CheckpointsReached);
    }

    [Fact]
    public void FullRound_TwoPlayersCollision_ShouldResolvePushing()
    {
        var engine = CreateEngineWithPlayers(2);
        
        var p1 = engine.State.Players[0];
        var p2 = engine.State.Players[1];
        
        p1.Robot.SetPosition(new Position(3, 1));
        p1.Robot.SetDirection(Direction.East);
        p2.Robot.SetPosition(new Position(5, 1));
        p2.Robot.SetDirection(Direction.West);
        
        p1.Registers[0] = new RegisterCard(new MoveCard { Priority = 100, Spaces = 2 }, 0);
        p2.Registers[0] = new RegisterCard(new MoveCard { Priority = 101, Spaces = 2 }, 0);
        
        engine.ExecuteRegister(0);
        
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
        
        engine.ExecuteRegister(0);
        
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
        var player = new Player { Id = 1, Robot = robot };
        
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
        var player = new Player { Id = 1, Robot = robot };
        
        dealer.Deal(new[] { player }, deck);
        
        Assert.Equal(0, player.AvailableCardCount);
    }

    [Fact]
    public void DealCards_With0Damage_ShouldDeal9Cards()
    {
        var dealer = new RandomDealer();
        var deck = CreateDefaultDeck();
        
        var player = new Player { Id = 1 };
        
        dealer.Deal(new[] { player }, deck);
        
        Assert.Equal(9, player.AvailableCardCount);
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
