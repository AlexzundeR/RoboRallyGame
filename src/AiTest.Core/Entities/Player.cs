namespace AiTest.Core.Entities;

using AiTest.Core.Cards;
using AiTest.Core.Upgrades;

public sealed record RegisterCard(ProgramCard Card, int HandIndex);

public sealed class Player
{
    private readonly RegisterCard?[] _registers;
    private readonly ProgramCard?[] _hand;

    public int Id { get; init; }
    public string Name { get; init; } = "Player";
    public Robot Robot { get; init; } = new();
    public IReadOnlyList<RegisterCard?> Registers => _registers;
    public IReadOnlyList<ProgramCard?> Hand => _hand;
    public List<UpgradeEffect> PermanentUpgrades { get; } = new();
    public List<UpgradeEffect> TemporaryUpgrades { get; } = new();

    public Player() : this(null, null) { }

    public Player(RegisterCard?[]? registers, ProgramCard?[]? hand)
    {
        _registers = registers ?? new RegisterCard?[5];
        _hand = hand ?? new ProgramCard?[9];
    }

    public IEnumerable<UpgradeEffect> AllUpgrades => PermanentUpgrades.Concat(TemporaryUpgrades);

    public int AvailableCardCount => _hand.Count(c => c != null);

    public void DealCards(IEnumerable<ProgramCard> cards)
    {
        for (int i = 0; i < 9; i++)
            _hand[i] = null;
        
        int index = 0;
        foreach (var card in cards)
        {
            if (index < 9)
            {
                _hand[index] = card;
                index++;
            }
        }
    }

    public RegisterCard? GetRegister(int index)
    {
        return index >= 0 && index < 5 ? _registers[index] : null;
    }

    public ProgramCard? GetHandCard(int index)
    {
        return index >= 0 && index < 9 ? _hand[index] : null;
    }

    public void AssignCardToRegister(int handIndex, int registerIndex)
    {
        if (handIndex < 0 || handIndex >= 9 || registerIndex < 0 || registerIndex >= 5)
            return;

        var card = _hand[handIndex];
        if (card == null) return;

        var registerCard = _registers[registerIndex];
        
        _hand[handIndex] = null;
        
        if (registerCard != null)
        {
            _hand[registerCard.HandIndex] = registerCard.Card;
        }
        
        _registers[registerIndex] = new RegisterCard(card, handIndex);
    }

    public void ClearRegisters()
    {
        for (int i = 0; i < 5; i++)
        {
            var registerCard = _registers[i];
            if (registerCard != null)
            {
                _hand[registerCard.HandIndex] = registerCard.Card;
            }
            _registers[i] = null;
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

    public int FilledRegisterCount => _registers.Count(r => r != null);

    public bool CanConfirmRegistration()
    {
        if (Robot.IsPowerDown)
            return FilledRegisterCount == 0;

        int availableRegisters = 5 - Robot.LockedRegisters;
        int availableCards = AvailableCardCount;
        int maxFillable = Math.Min(availableRegisters, availableCards);

        return FilledRegisterCount >= maxFillable;
    }

    public void TogglePowerDown()
    {
        if (Robot.IsPowerDown)
        {
            Robot.WakeUp();
        }
        else
        {
            ClearRegisters();
            Robot.PowerDown();
        }
    }
}
