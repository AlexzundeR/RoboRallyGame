namespace AiTest.Godot.Tests;

using AiTest.Core.Config;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public static class TestConfigs
{
    public static GameConfiguration CreateDefault2Player()
    {
        return new GameConfiguration
        {
            MapWidth = 10,
            MapHeight = 10,
            Players = new List<PlayerConfig>
            {
                new() { Name = "Player1", StartPosition = new Position(1, 1) },
                new() { Name = "Player2", StartPosition = new Position(8, 1) }
            },
            Map = new MapConfig
            {
                Checkpoints = new List<CheckpointPlacement>
                {
                    new() { X = 5, Y = 5, Number = 1 },
                    new() { X = 6, Y = 5, Number = 2 },
                    new() { X = 7, Y = 5, Number = 3 },
                    new() { X = 8, Y = 5, Number = 4 }
                },
                Starts = new List<StartPlacement>
                {
                    new() { X = 1, Y = 1 },
                    new() { X = 8, Y = 1 },
                    new() { X = 1, Y = 8 },
                    new() { X = 8, Y = 8 }
                }
            }
        };
    }
}
