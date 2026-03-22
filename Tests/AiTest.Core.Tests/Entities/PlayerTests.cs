namespace AiTest.Core.Tests.Entities;

using AiTest.Core.Cards;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public class PlayerTests
{
    private static Player CreateWithHand(params ProgramCard[] cards)
    {
        var hand = new ProgramCard?[9];
        for (int i = 0; i < cards.Length && i < 9; i++)
            hand[i] = cards[i];
        return new Player(null, hand);
    }

    [Fact]
    public void NewPlayer_ShouldHaveDefaultValues()
    {
        var player = new Player { Id = 1, Name = "TestPlayer" };
        
        Assert.Equal(1, player.Id);
        Assert.Equal("TestPlayer", player.Name);
        Assert.NotNull(player.Robot);
        for (int i = 0; i < 9; i++)
            Assert.Null(player.Hand[i]);
        for (int i = 0; i < 5; i++)
            Assert.Null(player.GetRegister(i));
    }

    [Fact]
    public void DealCards_ShouldReplaceHand()
    {
        var player = new Player();
        var cards = new List<ProgramCard>
        {
            new MoveCard { Spaces = 1, Priority = 100 },
            new TurnCard { IsLeft = true, Priority = 200 }
        };
        
        player.DealCards(cards);
        
        Assert.Equal(2, player.AvailableCardCount);
        Assert.NotNull(player.Hand[0]);
        Assert.NotNull(player.Hand[1]);
    }

    [Fact]
    public void GetRegister_InvalidIndex_ShouldReturnNull()
    {
        var player = new Player();
        
        Assert.Null(player.GetRegister(-1));
        Assert.Null(player.GetRegister(5));
    }

    [Fact]
    public void ClearRegisters_ShouldReturnCardsToHand()
    {
        var player = CreateWithHand(
            new MoveCard { Spaces = 1 },
            new TurnCard { IsLeft = true }
        );
        
        player.AssignCardToRegister(0, 0);
        player.AssignCardToRegister(1, 2);
        
        player.ClearRegisters();
        
        Assert.Null(player.GetRegister(0));
        Assert.Null(player.GetRegister(2));
        Assert.Equal(2, player.AvailableCardCount);
    }

    [Fact]
    public void AssignCardToRegister_EmptyRegister_ShouldMoveCardToRegister()
    {
        var player = CreateWithHand(new MoveCard { Spaces = 1, Priority = 100 });
        
        player.AssignCardToRegister(0, 0);
        
        var registerCard = player.GetRegister(0);
        Assert.NotNull(registerCard);
        Assert.Equal(100, registerCard.Card.Priority);
        Assert.Equal(0, registerCard.HandIndex);
        Assert.Null(player.Hand[0]);
    }

    [Fact]
    public void AssignCardToRegister_WithRegisterCard_ShouldSwap()
    {
        var player = CreateWithHand(
            new MoveCard { Spaces = 1, Priority = 100 },
            new MoveCard { Spaces = 2, Priority = 200 }
        );
        
        player.AssignCardToRegister(1, 0);
        Assert.Equal(200, player.GetRegister(0)!.Card.Priority);
        Assert.Equal(1, player.GetRegister(0)!.HandIndex);
        Assert.Null(player.Hand[1]);
        
        player.AssignCardToRegister(0, 1);
        
        Assert.Equal(200, player.GetRegister(0)!.Card.Priority);
        Assert.Equal(100, player.GetRegister(1)!.Card.Priority);
        Assert.Null(player.Hand[0]);
    }

    [Fact]
    public void AssignCardToRegister_OverwriteRegister_ShouldReturnOldCard()
    {
        var player = CreateWithHand(
            new MoveCard { Spaces = 1, Priority = 100 },
            new MoveCard { Spaces = 2, Priority = 200 },
            new MoveCard { Spaces = 3, Priority = 300 }
        );
        
        player.AssignCardToRegister(1, 0);
        Assert.Null(player.Hand[1]);
        Assert.Equal(200, player.GetRegister(0)!.Card.Priority);
        
        player.AssignCardToRegister(0, 0);
        
        Assert.Equal(100, player.GetRegister(0)!.Card.Priority);
        Assert.Equal(0, player.GetRegister(0)!.HandIndex);
        Assert.Equal(200, player.Hand[1]!.Priority);
    }

    [Fact]
    public void AssignCardToRegister_MultipleCards_ShouldWorkCorrectly()
    {
        var player = CreateWithHand(
            new MoveCard { Spaces = 1, Priority = 100 },
            new MoveCard { Spaces = 2, Priority = 200 },
            new MoveCard { Spaces = 3, Priority = 300 }
        );
        
        player.AssignCardToRegister(0, 0);
        player.AssignCardToRegister(1, 1);
        
        Assert.NotNull(player.GetRegister(0));
        Assert.NotNull(player.GetRegister(1));
        Assert.Equal(100, player.GetRegister(0)!.Card.Priority);
        Assert.Equal(0, player.GetRegister(0)!.HandIndex);
        Assert.Equal(200, player.GetRegister(1)!.Card.Priority);
        Assert.Equal(1, player.GetRegister(1)!.HandIndex);
        Assert.Null(player.Hand[0]);
        Assert.Null(player.Hand[1]);
        Assert.NotNull(player.Hand[2]);
    }

    [Fact]
    public void AssignCardToRegister_InvalidIndices_ShouldDoNothing()
    {
        var player = CreateWithHand(new MoveCard { Spaces = 1, Priority = 100 });
        
        player.AssignCardToRegister(-1, 0);
        player.AssignCardToRegister(0, 5);
        
        Assert.NotNull(player.Hand[0]);
        Assert.Equal(100, player.Hand[0].Priority);
    }

    [Fact]
    public void AssignCardToRegister_LockedRegister_ShouldNotAllowAssignment()
    {
        var robot = new Robot();
        robot.AddDamage(6);
        
        var hand = new ProgramCard?[9];
        hand[0] = new MoveCard { Priority = 100, Spaces = 1 };
        hand[1] = new MoveCard { Priority = 101, Spaces = 2 };
        
        var player = new Player(null, hand) { Robot = robot };
        
        player.AssignCardToRegister(0, 0);
        player.AssignCardToRegister(1, 1);
        
        Assert.NotNull(player.Hand[0]);
        Assert.NotNull(player.Hand[1]);
        Assert.Null(player.GetRegister(0));
        Assert.Null(player.GetRegister(1));
    }

    [Fact]
    public void AddEnergy_ShouldNotExceedMax()
    {
        var robot = new Robot();
        robot.AddEnergy(8);
        var player = new Player { Robot = robot };
        
        player.AddEnergy(5);
        
        Assert.Equal(10, player.Robot.Energy);
    }

    [Fact]
    public void SpendEnergy_ShouldNotGoBelowZero()
    {
        var robot = new Robot();
        robot.AddEnergy(2);
        var player = new Player { Robot = robot };
        
        player.SpendEnergy(5);
        
        Assert.Equal(0, player.Robot.Energy);
    }
}
