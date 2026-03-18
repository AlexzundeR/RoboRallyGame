namespace AiTest.Core.Entities;

using AiTest.Core.Cards;
using AiTest.Core.Upgrades;

public sealed class Player
{
    public int Id { get; init; }
    public string Name { get; init; } = "Player";
    public Robot Robot { get; init; } = new();
    public ProgramCard?[] Registers { get; } = new ProgramCard?[5];
    public List<ProgramCard> Hand { get; } = new();
    public List<UpgradeEffect> PermanentUpgrades { get; } = new();
    public List<UpgradeEffect> TemporaryUpgrades { get; } = new();
    
    private readonly HashSet<int> _usedHandSlots = new();

    public IEnumerable<UpgradeEffect> AllUpgrades => PermanentUpgrades.Concat(TemporaryUpgrades);

    public IReadOnlyCollection<int> UsedHandSlots => _usedHandSlots;

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

    public void AssignCardToRegister(ProgramCard card, int handIndex, int registerIndex)
    {
        if (handIndex < 0 || handIndex >= Hand.Count || registerIndex < 0 || registerIndex >= 5)
            return;

        var cardInReg = Registers[registerIndex];
        Hand.RemoveAt(handIndex);
        Registers[registerIndex] = card;
        _usedHandSlots.Add(handIndex);

        if (cardInReg != null)
            Hand.Add(cardInReg);
    }

    public void ClearRegisters()
    {
        for (int i = 0; i < 5; i++)
            Registers[i] = null;
        _usedHandSlots.Clear();
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
