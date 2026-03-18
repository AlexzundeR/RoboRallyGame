namespace AiTest.Core.Entities;

using AiTest.Core.Cards;
using AiTest.Core.Upgrades;

public sealed record RegisterCard(ProgramCard Card, int HandIndex);

public sealed class Player
{
    public int Id { get; init; }
    public string Name { get; init; } = "Player";
    public Robot Robot { get; init; } = new();
    public RegisterCard?[] Registers { get; }
    public ProgramCard?[] Hand { get; } = new ProgramCard?[9];
    public List<UpgradeEffect> PermanentUpgrades { get; } = new();
    public List<UpgradeEffect> TemporaryUpgrades { get; } = new();

    public Player() : this(null) { }

    public Player(RegisterCard?[]? registers)
    {
        Registers = registers ?? new RegisterCard?[5];
    }

    public static Player CreateWithRegisters(params (ProgramCard Card, int HandIndex)[] registers)
    {
        var player = new Player();
        for (int i = 0; i < registers.Length && i < 5; i++)
        {
            player.Registers[i] = new RegisterCard(registers[i].Card, registers[i].HandIndex);
        }
        return player;
    }

    public IEnumerable<UpgradeEffect> AllUpgrades => PermanentUpgrades.Concat(TemporaryUpgrades);

    public int AvailableCardCount => Hand.Count(c => c != null);

    public void DealCards(IEnumerable<ProgramCard> cards)
    {
        for (int i = 0; i < 9; i++)
            Hand[i] = null;
        
        int index = 0;
        foreach (var card in cards)
        {
            if (index < 9)
            {
                Hand[index] = card;
                index++;
            }
        }
    }

    public RegisterCard? GetRegister(int index)
    {
        return index >= 0 && index < 5 ? Registers[index] : null;
    }

    public ProgramCard? GetHandCard(int index)
    {
        return index >= 0 && index < 9 ? Hand[index] : null;
    }

    public void AssignCardToRegister(int handIndex, int registerIndex)
    {
        if (handIndex < 0 || handIndex >= 9 || registerIndex < 0 || registerIndex >= 5)
            return;

        var card = Hand[handIndex];
        if (card == null) return;

        var registerCard = Registers[registerIndex];
        
        Hand[handIndex] = null;
        
        if (registerCard != null)
        {
            Hand[registerCard.HandIndex] = registerCard.Card;
        }
        
        Registers[registerIndex] = new RegisterCard(card, handIndex);
    }

    public void ClearRegisters()
    {
        for (int i = 0; i < 5; i++)
        {
            var registerCard = Registers[i];
            if (registerCard != null)
            {
                Hand[registerCard.HandIndex] = registerCard.Card;
            }
            Registers[i] = null;
        }
    }

    public void AddEnergy(int amount) => Robot.AddEnergy(amount);
    public void SpendEnergy(int amount) => Robot.SpendEnergy(amount);

    public void AddUpgrade(UpgradeEffect upgrade)
    {
        if (upgrade.Type == UpgradeType.Permanent)
            PermanentUpgrades.Add(upgrade);
        else
            TemporaryUpgrades.Add(upgrade);
    }

    public void RemoveUpgrade(UpgradeEffect upgrade)
    {
        PermanentUpgrades.Remove(upgrade);
        TemporaryUpgrades.Remove(upgrade);
    }

    public void ClearTemporaryUpgrades() => TemporaryUpgrades.Clear();
}
