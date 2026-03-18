namespace AiTest.Core.Tests.Entities;

using AiTest.Core.Cards;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public class PlayerTests
{
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
    public void SetRegister_ShouldStoreCard()
    {
        var player = new Player();
        var card = new MoveCard { Spaces = 1, Priority = 100 };
        
        player.SetRegister(0, card);
        
        var registerCard = player.GetRegister(0);
        Assert.NotNull(registerCard);
        Assert.Equal(card, registerCard.Card);
        Assert.Equal(-1, registerCard.HandIndex);
    }

    [Fact]
    public void GetRegister_InvalidIndex_ShouldReturnNull()
    {
        var player = new Player();
        
        Assert.Null(player.GetRegister(-1));
        Assert.Null(player.GetRegister(5));
    }

    [Fact]
    public void ClearRegisters_ShouldSetAllToNull()
    {
        var player = new Player();
        player.SetRegister(0, new MoveCard { Spaces = 1 });
        player.SetRegister(2, new TurnCard { IsLeft = true });
        
        player.ClearRegisters();
        
        Assert.Null(player.GetRegister(0));
        Assert.Null(player.GetRegister(2));
    }

    [Fact]
    public void AssignCardToRegister_EmptyRegister_ShouldMoveCardToRegister()
    {
        var player = new Player();
        var card = new MoveCard { Spaces = 1, Priority = 100 };
        player.Hand[0] = card;
        
        player.AssignCardToRegister(0, 0);
        
        var registerCard = player.GetRegister(0);
        Assert.NotNull(registerCard);
        Assert.Equal(card, registerCard.Card);
        Assert.Equal(0, registerCard.HandIndex);
        Assert.Null(player.Hand[0]);
    }

    [Fact]
    public void AssignCardToRegister_WithExistingCard_ShouldSwap()
    {
        var player = new Player();
        var card1 = new MoveCard { Spaces = 1, Priority = 100 };
        var card2 = new MoveCard { Spaces = 2, Priority = 200 };
        
        player.Hand[0] = card1;
        player.SetRegister(0, card2);
        
        player.AssignCardToRegister(0, 0);
        
        var registerCard = player.GetRegister(0);
        Assert.NotNull(registerCard);
        Assert.Equal(card1, registerCard.Card);
        Assert.Equal(0, registerCard.HandIndex);
        Assert.Equal(card2, player.Hand[0]);
    }

    [Fact]
    public void AssignCardToRegister_WithRegisterCard_ShouldSwap()
    {
        var player = new Player();
        var card1 = new MoveCard { Spaces = 1, Priority = 100 };
        var card2 = new MoveCard { Spaces = 2, Priority = 200 };
        
        player.Hand[0] = card1;
        player.Hand[1] = card2;
        
        player.AssignCardToRegister(1, 0);
        Assert.Equal(card2, player.GetRegister(0)!.Card);
        Assert.Equal(1, player.GetRegister(0)!.HandIndex);
        Assert.Null(player.Hand[1]);
        
        player.AssignCardToRegister(0, 1);
        
        var register0 = player.GetRegister(0);
        Assert.NotNull(register0);
        Assert.Equal(card2, register0.Card);
        Assert.Equal(1, register0.HandIndex);
        
        var register1 = player.GetRegister(1);
        Assert.NotNull(register1);
        Assert.Equal(card1, register1.Card);
        Assert.Equal(0, register1.HandIndex);
        
        Assert.Null(player.Hand[0]);
        Assert.Null(player.Hand[1]);
    }

    [Fact]
    public void AssignCardToRegister_MultipleCards_ShouldSwapCorrectly()
    {
        var player = new Player();
        var card1 = new MoveCard { Spaces = 1, Priority = 100 };
        var card2 = new MoveCard { Spaces = 2, Priority = 200 };
        var card3 = new MoveCard { Spaces = 3, Priority = 300 };
        
        player.Hand[0] = card1;
        player.Hand[1] = card2;
        player.Hand[2] = card3;
        
        player.AssignCardToRegister(0, 0);
        player.AssignCardToRegister(1, 1);
        
        Assert.NotNull(player.GetRegister(0));
        Assert.NotNull(player.GetRegister(1));
        Assert.Equal(card1, player.GetRegister(0)!.Card);
        Assert.Equal(0, player.GetRegister(0)!.HandIndex);
        Assert.Equal(card2, player.GetRegister(1)!.Card);
        Assert.Equal(1, player.GetRegister(1)!.HandIndex);
        Assert.Null(player.Hand[0]);
        Assert.Null(player.Hand[1]);
        Assert.NotNull(player.Hand[2]);
    }

    [Fact]
    public void AssignCardToRegister_InvalidIndices_ShouldDoNothing()
    {
        var player = new Player();
        var card = new MoveCard { Spaces = 1, Priority = 100 };
        player.Hand[0] = card;
        
        player.AssignCardToRegister(-1, 0);
        player.AssignCardToRegister(0, 5);
        
        Assert.NotNull(player.Hand[0]);
        Assert.Equal(card, player.Hand[0]);
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
