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
            p1.AssignCardToRegister(i, i);
            p2.AssignCardToRegister(i + 5, i);
        }

        for (int reg = 0; reg < 5; reg++)
        {
            runner.Engine.ExecuteRegister(reg);
        }

        Assert.Equal(GamePhase.Programming, runner.Engine.State.Phase);
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
