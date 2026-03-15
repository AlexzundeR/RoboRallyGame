using AiTest.ConsoleApp;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;

var engine = new GameEngine();
var map = GameMap.CreateStandardMap();
engine.Initialize(map);

engine.AddPlayer("Alice");
engine.AddPlayer("Bob");

engine.StartGame();

ConsoleRenderer.RenderGame(engine);

for (int round = 1; round <= 3 && !engine.IsGameOver(); round++)
{
    Console.WriteLine($"\n=== Round {round} ===");
    
    foreach (var player in engine.State.Players)
    {
        for (int i = 0; i < 5; i++)
        {
            if (player.Hand.Count > i)
                player.SetRegister(i, player.Hand[i]);
        }
    }
    
    for (int r = 0; r < 5; r++)
    {
        engine.State.CurrentRegister = r;
        engine.ExecuteRegister(r);
        ConsoleRenderer.RenderGame(engine);
        Thread.Sleep(200);
    }
    
    engine.NextRound();
}

Console.WriteLine($"\nGame ended after {engine.State.Round} rounds.");
var winner = engine.GetWinner();
Console.WriteLine($"Winner: {winner?.Name ?? "No one"}");
