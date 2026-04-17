namespace AiTest.Tests.GdUnit.UI;

using AiTest.Godot.UI;
using GdUnit4;
using static GdUnit4.Assertions;
using System.Reflection;
using Godot;

[TestSuite]
public class RegisterActionCardTests
{
    [TestCase]
    [RequireGodotRuntime]
    public void LoadScene_ShouldWork()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://RegisterActionCard.tscn");
        AssertThat(scene).IsNotNull();
        
        var card = scene.Instantiate<RegisterActionCard>();
        AssertThat(card).IsNotNull();
        
        card.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CardSymbol_Default_ShouldBeNull()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://RegisterActionCard.tscn");
        var card = scene.Instantiate<RegisterActionCard>();
        
        AssertThat(card.CardSymbol).IsNull();
        
        card.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsEmpty_WhenNoSymbol_ShouldBeTrue()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://RegisterActionCard.tscn");
        var card = scene.Instantiate<RegisterActionCard>();
        
        AssertThat(card.IsEmpty).IsTrue();
        
        card.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void CardPriority_Default_ShouldBe100()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://RegisterActionCard.tscn");
        var card = scene.Instantiate<RegisterActionCard>();
        
        AssertThat(card.CardPriority).IsEqual(100);
        
        card.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsSelected_Default_ShouldBeFalse()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://RegisterActionCard.tscn");
        var card = scene.Instantiate<RegisterActionCard>();
        
        AssertThat(card.IsSelected).IsFalse();
        
        card.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsFixed_Default_ShouldBeFalse()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://RegisterActionCard.tscn");
        var card = scene.Instantiate<RegisterActionCard>();
        
        AssertThat(card.IsFixed).IsFalse();
        
        card.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void SetCardData_ViaReflection_ShouldUpdateProperties()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://RegisterActionCard.tscn");
        var card = scene.Instantiate<RegisterActionCard>();
        
        var method = typeof(RegisterActionCard).GetMethod("SetCardData", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(card, new object[] { "M1", 100, 1 });
        
        AssertThat(card.CardSymbol).IsEqual("M1");
        AssertThat(card.CardPriority).IsEqual(100);
        AssertThat(card.CardNumber).IsEqual(1);
        AssertThat(card.IsEmpty).IsFalse();
        
        card.QueueFree();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void SetEmpty_ViaReflection_ShouldClearProperties()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://RegisterActionCard.tscn");
        var card = scene.Instantiate<RegisterActionCard>();
        
        var setCardMethod = typeof(RegisterActionCard).GetMethod("SetCardData", BindingFlags.NonPublic | BindingFlags.Instance);
        setCardMethod?.Invoke(card, new object[] { "M1", 100, 1 });
        
        var setEmptyMethod = typeof(RegisterActionCard).GetMethod("SetEmpty", BindingFlags.NonPublic | BindingFlags.Instance);
        setEmptyMethod?.Invoke(card, null);
        
        AssertThat(card.CardSymbol).IsNull();
        AssertThat(card.IsEmpty).IsTrue();
        
        card.QueueFree();
    }
}
