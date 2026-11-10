namespace RoboRallyGame.Core.Entities;

using System.Collections.Generic;

using RoboRallyGame.Core.Cards;
using RoboRallyGame.Core.Upgrades;

public sealed class RegisterCard
{
    public ProgramCard? Card { get; private set; }
    public int HandIndex { get; private set; }
    public bool IsLocked { get; private set; } = false;

    public RegisterCard()
    {
        this.Card = null;
    }

    public RegisterCard(ProgramCard card, int handIndex)
    {
        SetCard(card, handIndex);
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public void Unlock()
    {
        IsLocked = false;
    }

    public void SetCard(ProgramCard card, int handIndex)
    {
        if (IsLocked) throw new Exception("Cannot set card on locked register");

        Card = card;
        HandIndex = handIndex;
    }

    public void Clear()
    {
        if (IsLocked) throw new Exception("Cannot clean locked register");

        Card = null;
    }
}

public sealed class Player
{
    //TODO: не должны ли регистры уйти в Robot?
    private readonly RegisterCard?[] _registers;
    private readonly ProgramCard?[] _hand;

    public int Id { get; init; }
    public string Name { get; init; } = "Player";
    public Robot Robot { get; init; } = new();
    public IReadOnlyList<RegisterCard?> Registers => _registers;
    public IReadOnlyList<ProgramCard?> Hand => _hand;
    public List<UpgradeEffect> PermanentUpgrades { get; } = new();
    public List<UpgradeEffect> TemporaryUpgrades { get; } = new();
    public int LockedRegisters => _registers.Count(e => e.IsLocked);

    public Player() : this(null, null) { }

    public Player(RegisterCard?[]? registers, ProgramCard?[]? hand)
    {
        _registers = new RegisterCard?[5];
        _hand = new ProgramCard?[9];

        for (int i = 0; i < _registers.Length; i++)
        {
            if (registers != null && i < registers?.Length && registers[i] != null)
                _registers[i] = new RegisterCard(registers[i].Card, registers[i].HandIndex);
            else
                _registers[i] = new RegisterCard();
        }

        if (hand != null)
        {
            for (int i = 0; i < (hand?.Length ?? 0); i++)
            {
                _hand[i] = hand[i];
            }
        }
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
        return index >= 0 && index < 5 ? _registers[index] : throw new Exception($"Register with index {index} was not found");
    }

    public ProgramCard? GetHandCard(int index)
    {
        return index >= 0 && index < 9 ? _hand[index] : throw new Exception($"Hand card with index {index} was not found");
    }

    public void AssignCardToRegister(int handIndex, int registerIndex)
    {
        if (handIndex < 0 || handIndex >= 9 || registerIndex < 0 || registerIndex >= 5)
            return;

        var card = _hand[handIndex];
        if (card == null) return;

        var registerCard = _registers[registerIndex];

        _hand[handIndex] = null;

        if (registerCard.Card != null)
        {
            _hand[registerCard.HandIndex] = registerCard.Card;
        }

        _registers[registerIndex].SetCard(card, handIndex);
    }

    public void ClearRegisters()
    {
        for (int i = 0; i < 5; i++)
        {
            var registerCard = _registers[i];
            if (registerCard.IsLocked) continue;

            if (registerCard.Card != null)
            {
                _hand[registerCard.HandIndex] = registerCard.Card;
            }
            _registers[i].Clear();
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

    public int FilledRegisterCount => _registers.Count(r => r.Card != null);

    public bool CanConfirmRegistration()
    {
        if (Robot.IsPowerDown || Robot.IsRebooting)
            return true;

        int availableRegisters = 5 - LockedRegisters;
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

    internal void TakeDamage(int damage)
    {
        Robot.TakeDamage(damage);

        if (_hand.Length - Robot.Damage < _registers.Length)
        {
            for (var i = 1; i <= _registers.Length - (_hand.Length - Robot.Damage); i++)
            {
                _registers[_registers.Length - i].Lock();
            }
        }
    }
}
