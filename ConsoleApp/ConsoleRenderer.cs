namespace AiTest.ConsoleApp;

using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public static class ConsoleRenderer
{
    private static readonly Dictionary<int, char> RobotChars = new()
    {
        { 1, '▲' }, { 2, '◆' }, { 3, '●' }, { 4, '■' },
        { 5, '★' }, { 6, '♦' }, { 7, '♠' }, { 8, '♣' }
    };

    public static void RenderGame(IGameEngine engine)
    {
        var map = engine.State.Map;
        var players = engine.State.Players;

        Console.WriteLine();
        Console.WriteLine($"=== RoboRally: {map.Name} ===");
        Console.WriteLine($"Round {engine.State.Round}, Register {engine.State.CurrentRegister + 1}");
        Console.WriteLine();

        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                var pos = new Position(x, y);
                var tile = map.GetTile(pos);
                var robot = players.FirstOrDefault(p => p.Robot.Position == pos && p.Robot.IsAlive);

                if (robot != null)
                {
                    var robotChar = RobotChars.GetValueOrDefault(robot.Id, '?');
                    Console.ForegroundColor = GetRobotColor(robot.Id);
                    Console.Write(robotChar);
                }
                else
                {
                    Console.ForegroundColor = GetTileColor(tile);
                    Console.Write(tile.ToChar());
                }
            }
            Console.WriteLine();
        }

        Console.ResetColor();
        Console.WriteLine();
        RenderStatus(engine);
    }

    private static void RenderStatus(IGameEngine engine)
    {
        Console.WriteLine("=== Players ===");
        foreach (var player in engine.State.Players)
        {
            var r = player.Robot;
            var status = r.Status.ToString()[..Math.Min(8, r.Status.ToString().Length)];
            var dir = r.Direction.ToSymbol();
            Console.WriteLine($"{player.Id}. {player.Name,-10} {dir} @ ({r.Position.X},{r.Position.Y}) HP:{r.Damage,2} E:{r.Energy} CP:{r.CheckpointsReached} [{status}]");
        }
    }

    private static ConsoleColor GetTileColor(Tile tile)
    {
        return tile.Type switch
        {
            TileType.Wall => ConsoleColor.DarkGray,
            TileType.Pit => ConsoleColor.Black,
            TileType.Start => ConsoleColor.Cyan,
            TileType.Checkpoint => ConsoleColor.Yellow,
            TileType.ConveyorCW or TileType.ConveyorCCW => ConsoleColor.Blue,
            TileType.ExpressConveyorCW or TileType.ExpressConveyorCCW => ConsoleColor.Magenta,
            TileType.GearCW or TileType.GearCCW => ConsoleColor.DarkYellow,
            TileType.LaserH or TileType.LaserV => ConsoleColor.Red,
            _ => ConsoleColor.Gray
        };
    }

    private static ConsoleColor GetRobotColor(int id)
    {
        return id switch
        {
            1 => ConsoleColor.Green,
            2 => ConsoleColor.Yellow,
            3 => ConsoleColor.Cyan,
            4 => ConsoleColor.Magenta,
            5 => ConsoleColor.White,
            6 => ConsoleColor.DarkGreen,
            7 => ConsoleColor.DarkCyan,
            8 => ConsoleColor.DarkMagenta,
            _ => ConsoleColor.Gray
        };
    }

    public static void ShowHand(Player player)
    {
        Console.WriteLine($"\n{player.Name}'s Hand:");
        for (int i = 0; i < player.Hand.Count; i++)
        {
            var card = player.Hand[i];
            var desc = card switch
            {
                MoveCard m => $"Move {m.Spaces} (P:{m.Priority})",
                TurnCard t => $"Turn {(t.IsLeft ? "Left" : "Right")} (P:{t.Priority})",
                UTurnCard => $"U-Turn (P:{card.Priority})",
                _ => "Unknown"
            };
            Console.WriteLine($"  {i + 1}. {desc}");
        }
    }

    public static void ShowRegisters(Player player)
    {
        Console.WriteLine("Registers:");
        for (int i = 0; i < 5; i++)
        {
            var card = player.GetRegister(i);
            var label = i < player.Robot.LockedRegisters ? "[LOCKED]" : $"[{i + 1}]";
            var desc = card != null ? GetCardDesc(card) : "Empty";
            Console.WriteLine($"  {label} {desc}");
        }
    }

    private static string GetCardDesc(ProgramCard card)
    {
        return card switch
        {
            MoveCard m => $"Move {m.Spaces} (P:{m.Priority})",
            TurnCard t => $"Turn {(t.IsLeft ? "Left" : "Right")} (P:{t.Priority})",
            UTurnCard => $"U-Turn (P:{card.Priority})",
            _ => "Unknown"
        };
    }
}
