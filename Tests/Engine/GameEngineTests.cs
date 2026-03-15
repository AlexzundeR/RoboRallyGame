namespace AiTest.Core.Tests.Engine;

using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public class GameEngineTests
{
    private GameEngine CreateEngine()
    {
        var engine = new GameEngine();
        var map = new GameMap(10, 10);
        map.SetTile(1, 1, Tile.Start);
        map.SetTile(8, 1, Tile.Start);
        map.SetTile(1, 8, Tile.Start);
        map.SetTile(8, 8, Tile.Start);
        map.SetTile(5, 5, Tile.Checkpoint(1));
        engine.Initialize(map);
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
        
        Assert.Equal(9, engine.State.Players[0].Hand.Count);
    }

    [Fact]
    public void ExecuteRegister_MoveCard_ShouldMoveRobot()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        engine.StartGame();
        
        var player = engine.State.Players[0];
        var moveCard = player.Hand.OfType<MoveCard>().First(c => c.Spaces == 1);
        player.SetRegister(0, moveCard);
        
        var startPos = player.Robot.Position;
        engine.ExecuteRegister(0);
        
        Assert.NotEqual(startPos, player.Robot.Position);
    }

    [Fact]
    public void ExecuteRegister_TurnCard_ShouldRotateRobot()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        engine.StartGame();
        
        var player = engine.State.Players[0];
        var turnCard = player.Hand.OfType<TurnCard>().First(c => c.IsLeft);
        player.SetRegister(0, turnCard);
        
        engine.ExecuteRegister(0);
        
        Assert.Equal(Direction.West, player.Robot.Direction);
    }

    [Fact]
    public void ProcessBoardElements_Gear_ShouldRotateRobot()
    {
        var engine = CreateEngine();
        engine.State.Map.SetTile(3, 3, Tile.Gear(true));
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.Position = new Position(3, 3);
        player.Robot.Direction = Direction.North;
        
        engine.ProcessBoardElements();
        
        Assert.Equal(Direction.East, player.Robot.Direction);
    }

    [Fact]
    public void ProcessBoardElements_Checkpoint_ShouldAdvanceCheckpoint()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.Position = new Position(5, 5);
        
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
        player.Robot.Position = new Position(3, 3);
        
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
        player.Robot.CheckpointsReached = 4;
        
        Assert.True(engine.IsGameOver());
    }

    [Fact]
    public void GetWinner_WhenPlayerWins_ShouldReturnPlayer()
    {
        var engine = CreateEngine();
        engine.AddPlayer("Player1");
        var player = engine.State.Players[0];
        player.Robot.CheckpointsReached = 4;
        
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
}
