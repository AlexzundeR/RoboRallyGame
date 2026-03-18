namespace AiTest.Core.Tests.Engine;

using AiTest.Core.Cards;
using AiTest.Core.Config;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;
using Xunit;

public class GameRunnerTests
{
    [Fact]
    public void CreateMinimal_DefaultConfiguration_ShouldHaveOnePlayer()
    {
        var config = GameRunner.CreateMinimal();

        Assert.Single(config.Players);
        Assert.Equal("Player1", config.Players[0].Name);
    }

    [Fact]
    public void CreateDefault2Player_DefaultConfiguration_ShouldHaveTwoPlayers()
    {
        var config = GameRunner.CreateDefault2Player();

        Assert.Equal(2, config.Players.Count);
        Assert.Equal("Player1", config.Players[0].Name);
        Assert.Equal("Player2", config.Players[1].Name);
    }

    [Fact]
    public void Initialize_WithMinimalConfig_ShouldCreateGame()
    {
        var runner = new GameRunner(GameRunner.CreateMinimal());
        
        runner.Initialize();

        Assert.NotNull(runner.Engine);
        Assert.Single(runner.Engine.State.Players);
    }

    [Fact]
    public void Initialize_With2PlayerConfig_ShouldCreateGameWith2Players()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        
        runner.Initialize();

        Assert.Equal(2, runner.Engine.State.Players.Count);
    }

    [Fact]
    public void Initialize_WithCheckpoints_ShouldPlaceCheckpointsOnMap()
    {
        var runner = new GameRunner(GameRunner.CreateMinimal());
        
        runner.Initialize();

        var tile = runner.Engine.State.Map.GetTile(3, 3);
        Assert.Equal(TileType.Checkpoint, tile.Type);
    }

    [Fact]
    public void Initialize_WithWalls_ShouldPlaceWallsOnMap()
    {
        var config = new GameConfiguration
        {
            MapWidth = 8,
            MapHeight = 8,
            Players = new List<PlayerConfig> { new() { Name = "Player1" } },
            Map = new MapConfig
            {
                Tiles = new List<TilePlacement>
                {
                    new() { X = 3, Y = 3, Type = TileType.Wall }
                }
            }
        };
        var runner = new GameRunner(config);
        
        runner.Initialize();

        var tile = runner.Engine.State.Map.GetTile(3, 3);
        Assert.Equal(TileType.Wall, tile.Type);
    }

    [Fact]
    public void FullRound_TwoPlayersProgramAndExecute_ShouldCompleteAllRegisters()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();

        var p1 = runner.Engine.State.Players[0];
        var p2 = runner.Engine.State.Players[1];

        for (int i = 0; i < 5; i++)
        {
            p1.SetRegister(i, new MoveCard { Priority = 100 + i, Spaces = 1 }, i);
            p2.SetRegister(i, new MoveCard { Priority = 200 + i, Spaces = 1 }, i);
        }

        for (int reg = 0; reg < 5; reg++)
        {
            runner.Engine.ExecuteRegister(reg);
        }

        Assert.Equal(GamePhase.Programming, runner.Engine.State.Phase);
    }

    [Fact]
    public void ExecuteRegister_MoveCard_ShouldMoveRobot()
    {
        var runner = new GameRunner(GameRunner.CreateMinimal());
        runner.Initialize();

        var player = runner.Engine.State.Players[0];
        var startPos = player.Robot.Position;
        
        player.SetRegister(0, new MoveCard { Priority = 100, Spaces = 1 }, 0);

        runner.Engine.ExecuteRegister(0);

        Assert.NotEqual(startPos, player.Robot.Position);
    }

    [Fact]
    public void ExecuteRegister_TurnCard_ShouldRotateRobot()
    {
        var runner = new GameRunner(GameRunner.CreateMinimal());
        runner.Initialize();

        var player = runner.Engine.State.Players[0];
        player.Robot.SetDirection(Direction.North);
        
        player.SetRegister(0, new TurnCard { Priority = 150, IsLeft = true }, 0);

        runner.Engine.ExecuteRegister(0);

        Assert.Equal(Direction.West, player.Robot.Direction);
    }

    [Fact]
    public void TwoPlayers_OneMovesFirst_ShouldExecuteByPriority()
    {
        var runner = new GameRunner(GameRunner.CreateDefault2Player());
        runner.Initialize();

        var p1 = runner.Engine.State.Players[0];
        var p2 = runner.Engine.State.Players[1];
        
        p1.SetRegister(0, new MoveCard { Priority = 100, Spaces = 1 }, 0);
        p2.SetRegister(0, new MoveCard { Priority = 200, Spaces = 1 }, 0);

        var p1Start = p1.Robot.Position;
        var p2Start = p2.Robot.Position;

        runner.Engine.ExecuteRegister(0);

        Assert.NotEqual(p1Start, p1.Robot.Position);
        Assert.NotEqual(p2Start, p2.Robot.Position);
    }

    [Fact]
    public void PlayerWithDamage_FewerCardsDealt_ShouldReceiveFewerCards()
    {
        var config = GameRunner.CreateMinimal();
        var runner = new GameRunner(config);
        runner.Initialize();

        var player = runner.Engine.State.Players[0];
        player.Robot.AddDamage(5);

        runner.Engine.NextRound();

        Assert.Equal(4, player.AvailableCardCount);
    }

    [Fact]
    public void PlayerWith9Damage_NoCardsDealt_ShouldReceiveZeroCards()
    {
        var config = GameRunner.CreateMinimal();
        var runner = new GameRunner(config);
        runner.Initialize();

        var player = runner.Engine.State.Players[0];
        player.Robot.AddDamage(9);

        runner.Engine.NextRound();

        Assert.Equal(0, player.AvailableCardCount);
    }
}
