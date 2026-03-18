using Godot;
using System.Collections.Generic;
using AiTest.Core.Entities;
using AiTest.Core.Models;

namespace AiTest.Godot;

public partial class GameUI : CanvasLayer
{
	private Label? _roundLabel;
	private Label? _phaseLabel;
	private Label? _priorityLabel;
	private Label? _registerLabel;
	private Label? _actionPrompt;
	private Label? _helpLabel;
	private Button? _infoButton;
	private PanelContainer? _statusModal;
	private Button? _closeButton;

	private Label? _p1Header;
	private Label? _p1Status;

	[Export] public PackedScene PlayerRegisterActionCard;

	private BoxContainer? _p1HandCardholder;
	private BoxContainer? _p1RegistersCardholder;
	//Todo: плохо, что вся логика игроков продублирована, нужно вынести панель игрока в отдельный контрол и переиспользовать его для каждого игрока
	private Label? _p2Header;
	private Label? _p2Hand;
	private Label? _p2Registers;
	private Label? _p2Status;

	private bool _modalVisible = false;

	public string PriorityText
	{
		set
		{
			if (_priorityLabel != null)
				_priorityLabel.Text = value;
		}
	}

	public override void _Ready()
	{
		_roundLabel = GetNode<Label>("TopPanel/HBox/LeftPanel/RoundLabel");
		_phaseLabel = GetNode<Label>("TopPanel/HBox/LeftPanel/PhaseLabel");
		_priorityLabel = GetNode<Label>("TopPanel/HBox/MiddlePanel/PriorityLabel");
		_registerLabel = GetNode<Label>("TopPanel/HBox/MiddlePanel/RegisterLabel");
		_actionPrompt = GetNode<Label>("TopPanel/HBox/MiddlePanel/ActionPrompt");
		_helpLabel = GetNode<Label>("TopPanel/HBox/RightPanel/HelpLabel");
		_infoButton = GetNode<Button>("TopPanel/HBox/InfoButton");
		_statusModal = GetNode<PanelContainer>("StatusModal");

		_p1Header = GetNode<Label>("BottomPanel/HBox/Player1Panel/P1Header");
		_p1Status = GetNode<Label>("BottomPanel/HBox/Player1Panel/P1Status");
		_p1HandCardholder = GetNode<BoxContainer>("BottomPanel/HBox/Player1Panel/P1HandRow/HandCardholder");
		_p1RegistersCardholder = GetNode<BoxContainer>("BottomPanel/HBox/Player1Panel/P1RegistersRow/RegistersCardholder");

		_p2Header = GetNode<Label>("BottomPanel/HBox/Player2Panel/P2Header");
		_p2Hand = GetNode<Label>("BottomPanel/HBox/Player2Panel/P2Hand");
		_p2Registers = GetNode<Label>("BottomPanel/HBox/Player2Panel/P2Registers");
		_p2Status = GetNode<Label>("BottomPanel/HBox/Player2Panel/P2Status");

		if (_infoButton != null)
			_infoButton.Pressed += OnInfoPressed;

		if (_closeButton != null)
			_closeButton.Pressed += OnInfoPressed;
	}

	private void OnInfoPressed()
	{
		_modalVisible = !_modalVisible;
		if (_statusModal != null)
			_statusModal.Visible = _modalVisible;
	}

	public void UpdateRound(int round)
	{
		if (_roundLabel != null)
			_roundLabel.Text = $"Round: {round}";
	}

	public void UpdatePhase(bool programming)
	{
		if (_phaseLabel == null) return;

		_phaseLabel.Text = programming ? "PROGRAMMING" : "EXECUTION";
		_phaseLabel.AddThemeColorOverride("font_color", programming ? Colors.Yellow : Colors.Green);
	}

	public void UpdateRegisterPrompt(int currentRegister, int selectedRegister, string playerName)
	{
		if (_registerLabel == null) return;

		var nextReg = currentRegister < 5 ? currentRegister + 1 : 5;
		_registerLabel.Text = $"{playerName}: Register [{selectedRegister + 1}]/5";

		if (_actionPrompt != null)
		{
			_actionPrompt.Text = $"🎯 {playerName.ToUpper()}: ←→ SELECT REGISTER | 1-9 SELECT CARD";
		}
	}

	public void UpdateExecutionPrompt(int currentRegister, string playerName)
	{
		if (_registerLabel == null) return;

		_registerLabel.Text = $"{playerName}: Executing register {currentRegister + 1}/5";

		if (_actionPrompt != null)
		{
			_actionPrompt.Text = $"⚡ {playerName.ToUpper()}: EXECUTING REGISTER {currentRegister + 1} (PRESS SPACE)";
		}
	}

	public void UpdatePlayerInfo(Player p1, Player p2, int currentRegister, bool programming, int currentPlayerIndex, int selectedRegister = 0)
	{
		if (_p1RegistersCardholder != null)
		{
			RedrawRegisters(p1, currentRegister, programming, currentPlayerIndex == 0, selectedRegister);
		}
		if (_p1Status != null)
			_p1Status.Text = FormatPlayerStatus(p1);
		if (_p1HandCardholder != null)
		{
			RedrawHand(p1, currentPlayerIndex == 0 && programming);
		}

		if (_p2Hand != null)
			_p2Hand.Text = $"Hand: {FormatHand(p2, currentPlayerIndex == 1 && programming)}";
		if (_p2Registers != null)
			_p2Registers.Text = $"Regs: {FormatRegisters(p2, currentRegister, programming, currentPlayerIndex == 1, selectedRegister)}";
		if (_p2Status != null)
			_p2Status.Text = FormatPlayerStatus(p2);
	}

	private string FormatHand(Player player, bool isCurrentPlayer)
	{
		var cards = new List<string>();
		for (int i = 0; i < 9; i++)
		{
			if (i < player.Hand.Count)
			{
				var symbol = GetCardSymbol(player.Hand[i]);
				var priority = player.Hand[i].Priority;
				var prefix = isCurrentPlayer ? $"[{i + 1}:" : "[";
				cards.Add($"{prefix}{priority}:{symbol}]");
			}
			else
			{
				cards.Add("[---]");
			}
		}
		return string.Join("", cards);
	}

	private void RedrawHand(Player player, bool isCurrentPlayer)
	{
		foreach (var card in _p1HandCardholder.GetChildren())
		{
			_p1HandCardholder.RemoveChild(card);
		}

		for (int i = 0; i < 9; i++)
		{
			var card = PlayerRegisterActionCard.Instantiate<RegisterActionCard>();

			if (i < player.Hand.Count)
			{
				var symbol = GetCardSymbol(player.Hand[i]);
				var priority = player.Hand[i].Priority;
				var number = i + 1;

				card.SetCardData(symbol, priority, number);
			}
			else
			{
				card.SetEmpty();
			}

			_p1HandCardholder.AddChild(card);
		}
	}

	private void RedrawRegisters(Player player, int currentRegister, bool programming, bool isCurrentPlayer, int selectedRegister = 0)
	{
		foreach (var card in _p1RegistersCardholder.GetChildren())
		{
			_p1RegistersCardholder.RemoveChild(card);
		}

		for (int i = 0; i < 5; i++)
		{
			var card = PlayerRegisterActionCard.Instantiate<RegisterActionCard>();

			if (!programming && i == currentRegister ||
			 programming && i == selectedRegister)
			{
				card.SetSelected(true);
			}
			
			if (!programming || !isCurrentPlayer)
			{
				card.SetFixed(true);
			}

			var registerCard = player.GetRegister(i);
			if (registerCard == null)
			{
				card.SetEmpty();
			}
			else
			{
				var symbol = GetCardSymbol(registerCard);
				var priority = registerCard.Priority;
				var number = i + 1;

				card.SetCardData(symbol, priority, number);
			}


			_p1RegistersCardholder.AddChild(card);
		}
	}

	private string FormatRegisters(Player player, int currentRegister, bool programming, bool isCurrentPlayer, int selectedRegister = 0)
	{
		var registers = new List<string>();
		for (int i = 0; i < 5; i++)
		{
			var prefix = "";
			var suffix = "";

			if (isCurrentPlayer)
			{
				if (!programming && i == currentRegister)
				{
					prefix = "▶";
					suffix = "◀";
				}
				else if (programming && i == selectedRegister)
				{
					prefix = "▼";
					suffix = "◀";
				}
			}

			var card = player.GetRegister(i);
			var cardStr = card == null ? "---" : $"{card.Priority}:{GetCardSymbol(card)}";
			registers.Add($"{prefix}{cardStr}{suffix}");
		}
		return $"[{string.Join("][", registers)}]";
	}

	private string FormatPlayerStatus(Player player)
	{
		var hp = player.Robot.LifeTokens;
		var damage = player.Robot.Damage;
		return $"❤️ HP: {hp}/3 | 💥 DMG: {damage}";
	}

	private string GetCardSymbol(ProgramCard card)
	{
		return card switch
		{
			MoveCard m => $"M{m.Spaces}",
			TurnCard t => t.IsLeft ? "↺" : "↻",
			UTurnCard => "⟲",
			_ => "?"
		};
	}

	private string GetDirectionSymbol(Direction direction)
	{
		return direction switch
		{
			Direction.North => "⬆",
			Direction.East => "➡",
			Direction.South => "⬇",
			Direction.West => "⬅",
			_ => "?"
		};
	}
}
