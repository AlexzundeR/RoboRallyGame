using Godot;
using System.Collections.Generic;
using AiTest.Core.Cards;
using AiTest.Core.Entities;

public partial class PlayerPanel : PanelContainer
{
	[Signal] public delegate void RegisterClickedEventHandler(int registerIndex);

	private Label _header;
	private HBoxContainer _handCardholder;
	private HBoxContainer _registersCardholder;
	private Label _status;
	private Theme _cardTheme;
	private PackedScene _cardScene;

	private readonly List<RegisterActionCard> _handCards = new();
	private readonly List<RegisterActionCard> _registerCards = new();

	public override void _Ready()
	{
		_header = GetNode<Label>("VBox/Header");
		_handCardholder = GetNode<HBoxContainer>("VBox/HandRow/HandCardholder");
		_registersCardholder = GetNode<HBoxContainer>("VBox/RegistersRow/RegistersCardholder");
		_status = GetNode<Label>("VBox/Status");

		_cardTheme = GD.Load<Theme>("res://card_theme.tres");
		_cardScene = GD.Load<PackedScene>("res://RegisterActionCard.tscn");

		InitializeCards();
	}

	private void InitializeCards()
	{
		for (int i = 0; i < 9; i++)
		{
			var card = _cardScene.Instantiate<RegisterActionCard>();
			card.CardTheme = _cardTheme;
			_handCardholder.AddChild(card);
			_handCards.Add(card);
		}

		for (int i = 0; i < 5; i++)
		{
			var card = _cardScene.Instantiate<RegisterActionCard>();
			card.CardTheme = _cardTheme;
			int index = i;
			card.GuiInput += (e) => OnRegisterCardInput(e, index);
			_registersCardholder.AddChild(card);
			_registerCards.Add(card);
		}
	}

	private void OnRegisterCardInput(InputEvent @event, int index)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			EmitSignal(SignalName.RegisterClicked, index);
		}
	}

	public void Update(Player player, bool isCurrentPlayer, bool isProgramming,
					   int currentRegister, int selectedRegister)
	{
		if (!IsInsideTree()) return;
		UpdateHeader(player);
		UpdateHand(player, isCurrentPlayer && isProgramming);
		UpdateRegisters(player, currentRegister, isProgramming, isCurrentPlayer, selectedRegister);
		UpdateStatus(player);
	}

	private void UpdateHeader(Player player)
	{
		var icon = player.Id == 1 ? "🔵" : "🔴";
		_header.Text = $"{icon} {player.Name}";
	}

	private void UpdateHand(Player player, bool isSelectable)
	{
		for (int i = 0; i < 9; i++)
		{
			var card = _handCards[i];
			var programCard = player.Hand[i];

			if (programCard == null)
			{
				card.SetEmpty();
			}
			else
			{
				var symbol = GetCardSymbol(programCard);
				card.SetCardData(symbol, programCard.Priority, i + 1);
			}
		}
	}

	private void UpdateRegisters(Player player, int currentRegister, bool isProgramming,
							   bool isCurrentPlayer, int selectedRegister)
	{
		bool isPowerDown = player.Robot.IsPowerDown;

		for (int i = 0; i < 5; i++)
		{
			var card = _registerCards[i];

			if (!isProgramming && i == currentRegister)
			{
				card.SetSelected(true);
			}
			else if (isProgramming && i == selectedRegister && isCurrentPlayer)
			{
				card.SetSelected(true);
			}
			else
			{
				card.SetSelected(false);
			}

			if (isPowerDown)
			{
				card.SetLocked(true);
			}
			else
			{
				card.SetLocked(false);
				if (!isProgramming || !isCurrentPlayer)
				{
					card.SetFixed(true);
				}
				else
				{
					card.SetFixed(false);
				}
			}

			var registerCard = player.GetRegister(i);
			if (registerCard == null || isPowerDown)
			{
				card.SetEmpty();
			}
			else
			{
				var symbol = GetCardSymbol(registerCard.Card);
				int displayNumber = registerCard.HandIndex >= 0 ? registerCard.HandIndex + 1 : i + 1;
				card.SetCardData(symbol, registerCard.Card.Priority, displayNumber);
			}
		}
	}

	private void UpdateStatus(Player player)
	{
		var hp = player.Robot.LifeTokens;
		var damage = player.Robot.Damage;
		var checkpoints = player.Robot.CheckpointsReached;
		var powerDown = player.Robot.IsPowerDown ? " | ⚡ OFF" : "";
		_status.Text = $"❤️ HP: {hp}/3 | 💥 DMG: {damage} | ⭐ {checkpoints}{powerDown}";
	}

	public void UpdateCheckpoints(int checkpointsReached, int totalCheckpoints)
	{
		if (!IsInsideTree()) return;
		_status.Text = _status.Text.Replace($"⭐ {checkpointsReached - 1}", $"⭐ {checkpointsReached}");
	}

	private string GetCardSymbol(ProgramCard card)
	{
		return card switch
		{
			MoveCard m => $"M{m.Spaces}",
			TurnCard t => t.IsLeft ? "↺" : "↻",
					UTurnCard => "⤵",
			BackUpCard => "⬇",
			_ => "?"
		};
	}
}
