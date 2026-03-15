namespace AiTest.Core.Entities;

using AiTest.Core.Models;

public enum RegisterIndex
{
    One = 0,
    Two = 1,
    Three = 2,
    Four = 3,
    Five = 4
}

public abstract class ProgramCard
{
    public int Priority { get; init; }
    public abstract void Execute(Robot robot, GameMap map);
}

public sealed class MoveCard : ProgramCard
{
    public int Spaces { get; init; }

    public override void Execute(Robot robot, GameMap map)
    {
        for (int i = 0; i < Spaces; i++)
        {
            var newPos = robot.Position.Move(robot.Direction);
            if (map.IsInBounds(newPos) && map.GetTile(newPos).Type != TileType.Wall)
            {
                robot.Move(newPos);
            }
            else
            {
                break;
            }
        }
    }
}

public sealed class TurnCard : ProgramCard
{
    public bool IsLeft { get; init; }

    public override void Execute(Robot robot, GameMap map)
    {
        if (IsLeft)
            robot.RotateLeft();
        else
            robot.RotateRight();
    }
}

public sealed class UTurnCard : ProgramCard
{
    public override void Execute(Robot robot, GameMap map)
    {
        robot.Rotate180();
    }
}

public sealed class Player
{
    public int Id { get; init; }
    public string Name { get; set; } = "Player";
    public Robot Robot { get; set; } = new();
    public ProgramCard?[] Registers { get; } = new ProgramCard?[5];
    public List<ProgramCard> Hand { get; } = new();
    public int Priority { get; set; }

    public void DealCards(IEnumerable<ProgramCard> cards)
    {
        Hand.Clear();
        Hand.AddRange(cards);
    }

    public void SetRegister(int index, ProgramCard? card)
    {
        if (index >= 0 && index < 5)
            Registers[index] = card;
    }

    public ProgramCard? GetRegister(int index)
    {
        return index >= 0 && index < 5 ? Registers[index] : null;
    }

    public void ClearRegisters()
    {
        for (int i = 0; i < 5; i++)
            Registers[i] = null;
    }

    public void AddEnergy(int amount) => Robot.Energy = Math.Min(Robot.Energy + amount, 10);
    public void SpendEnergy(int amount) => Robot.Energy = Math.Max(Robot.Energy - amount, 0);
}
