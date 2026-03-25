namespace AiTest.Core.Config;

using AiTest.Core.Cards;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public class GameConfiguration
{
    public int MapWidth { get; init; } = 12;
    public int MapHeight { get; init; } = 12;
    public List<PlayerConfig> Players { get; init; } = new();
    public MapConfig Map { get; init; } = new();
    public DeckConfig Deck { get; init; } = new();
}

public class PlayerConfig
{
    public string Name { get; init; } = "Player";
    public Position? StartPosition { get; init; }
    public Direction StartDirection { get; init; } = Direction.North;
}

public class MapConfig
{
    public List<TilePlacement> Tiles { get; init; } = new();
    public List<CheckpointPlacement> Checkpoints { get; init; } = new();
    public List<StartPlacement> Starts { get; init; } = new();
}

public class TilePlacement
{
    public int X { get; init; }
    public int Y { get; init; }
    public TileType Type { get; init; }
    public List<Direction>? BlockedDirections { get; init; }
    public Direction? ConveyorDir { get; init; }
    public bool? IsExpress { get; init; }
    public bool? IsClockwise { get; init; }
    public Direction? LaserDirection { get; init; }
}

public class CheckpointPlacement
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Number { get; init; }
}

public class StartPlacement
{
    public int X { get; init; }
    public int Y { get; init; }
}

public class DeckConfig
{
    public bool UseFullDeck { get; init; } = true;
    public List<ProgramCard>? CustomCards { get; init; }
}
