using Godot;
using System.Collections.Generic;
using System.Linq;
using RoboRallyGame.Godot;

public partial class PlayerPanel : PanelContainer
{
	[Signal] public delegate void RegisterClickedEventHandler(int registerIndex);
	[Signal] public delegate void UpgradesClickedEventHandler();

	private Label _header;
	private HBoxContainer _handCardholder;
	private HBoxContainer _registersCardholder;
	private Label _status;
	private Theme _cardTheme;
	private PackedScene _cardScene;
	private bool _initialized;

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

		_status.GuiInput += OnStatusClicked;
		
		InitializeCards();

		_initialized = true;
	}

	private void OnStatusClicked(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			EmitSignal(SignalName.UpgradesClicked);
		}
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

	public void Update(PlayerViewModel player, bool isCurrentPlayer, bool isProgramming,
					   int currentRegister, int selectedRegister, int totalCheckpoints)
	{
		if (!IsInsideTree()) return;
		UpdateHeader(player);
		UpdateHand(player, isCurrentPlayer && isProgramming);
		UpdateRegisters(player, currentRegister, isProgramming, isCurrentPlayer, selectedRegister);
		UpdateStatus(player);
		UpdateCheckpoints(player.CheckpointsReached, totalCheckpoints);
	}

	private void UpdateHeader(PlayerViewModel player)
	{
		var icon = player.Id switch
		{
			1 => "🔵",
			2 => "🔴",
			3 => "🟡",
			4 => "⚪",
		};
		_header.Text = $"{icon} {player.Name}";
	}

	private void UpdateHand(PlayerViewModel player, bool isSelectable)
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

	private void UpdateRegisters(PlayerViewModel player, int currentRegister, bool isProgramming,
							   bool isCurrentPlayer, int selectedRegister)
	{
		if (!_initialized) return;

		for (int i = 0; i < 5; i++)
		{
			var cardData = player.Registers[i];
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

			if (player.IsPowerDown || player.IsRebooting || cardData.IsLocked)
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

			var registerCard = player.Registers[i];
			if (registerCard?.Card == null || player.IsPowerDown || player.IsRebooting)
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

	private void UpdateStatus(PlayerViewModel player)
	{
		var hp = player.LifeTokens;
		var damage = player.Damage;
		var checkpoints = player.CheckpointsReached;
		var upgrades = player.AllUpgrades.ToList();
		var powerDown = player.IsPowerDown ? " | ⚡ OFF" : "";
		var rebooting = player.IsRebooting ? " | ⚡ REBOOT" : "";
		
		string upgradeText;
		if (upgrades.Count == 0)
		{
			upgradeText = "";
		}
		else if (upgrades.Count == 1)
		{
			upgradeText = $" | ⚙ {upgrades[0]}";
		}
		else
		{
			upgradeText = $" | ⚙ {upgrades.Count} upgrades";
		}
		
		_status.Text = $"❤️ HP: {hp}/3 | 💥 DMG: {damage} | ⭐ {checkpoints}{upgradeText}{powerDown}{rebooting}";
	}

	private void UpdateCheckpoints(int checkpointsReached, int totalCheckpoints)
	{
		if (!IsInsideTree()) return;
		_status.Text = _status.Text.Replace($"⭐ {checkpointsReached - 1}", $"⭐ {checkpointsReached}");
	}

	private string GetCardSymbol(CardViewModel card) => card.Symbol;
}
