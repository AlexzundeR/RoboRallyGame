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
		private Label? _helpLabel;
		private Button? _infoButton;
		private PanelContainer? _statusModal;
		private Button? _closeButton;

		private Label? _p1Header;
		private Label? _p1Hand;
		private Label? _p1Registers;
		private Label? _p1Status;
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
			_helpLabel = GetNode<Label>("TopPanel/HBox/RightPanel/HelpLabel");
			_infoButton = GetNode<Button>("TopPanel/HBox/InfoButton");
			_statusModal = GetNode<PanelContainer>("StatusModal");

			_p1Header = GetNode<Label>("BottomPanel/HBox/Player1Panel/P1Header");
			_p1Hand = GetNode<Label>("BottomPanel/HBox/Player1Panel/P1Hand");
			_p1Registers = GetNode<Label>("BottomPanel/HBox/Player1Panel/P1Registers");
			_p1Status = GetNode<Label>("BottomPanel/HBox/Player1Panel/P1Status");

			_p2Header = GetNode<Label>("BottomPanel/HBox/Player2Panel/P2Header");
			_p2Hand = GetNode<Label>("BottomPanel/HBox/Player2Panel/P2Hand");
			_p2Registers = GetNode<Label>("BottomPanel/HBox/Player2Panel/P2Registers");
			_p2Status = GetNode<Label>("BottomPanel/HBox/Player2Panel/P2Status");

			GD.Print("[GameUI] Labels found: round=" + (_roundLabel != null) + 
				", p1Hand=" + (_p1Hand != null) + ", p2Hand=" + (_p2Hand != null) +
				", closeBtn=" + (_closeButton != null) + ", modal=" + (_statusModal != null));

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

	public void UpdateRegisterPrompt(int currentRegister, string playerName)
	{
		if (_registerLabel == null) return;

		var nextReg = currentRegister < 5 ? currentRegister + 1 : 5;
		_registerLabel.Text = $"{playerName}: Select register {nextReg}/5";
	}

	public void UpdateExecutionPrompt(int currentRegister, string playerName)
	{
		if (_registerLabel == null) return;

		_registerLabel.Text = $"{playerName}: Executing register {currentRegister + 1}/5";
	}

	public void UpdatePlayerInfo(Player p1, Player p2, int currentRegister, bool programming, int currentPlayerIndex)
	{
		if (_p1Hand != null)
			_p1Hand.Text = $"Hand: {FormatHand(p1, currentPlayerIndex == 0 && programming)}";
		if (_p1Registers != null)
			_p1Registers.Text = $"Regs: {FormatRegisters(p1, currentRegister, programming, currentPlayerIndex == 0)}";
		if (_p1Status != null)
			_p1Status.Text = FormatPlayerStatus(p1);

		if (_p2Hand != null)
			_p2Hand.Text = $"Hand: {FormatHand(p2, currentPlayerIndex == 1 && programming)}";
		if (_p2Registers != null)
			_p2Registers.Text = $"Regs: {FormatRegisters(p2, currentRegister, programming, currentPlayerIndex == 1)}";
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
				var prefix = isCurrentPlayer ? $"[{i + 1}:" : "[";
				cards.Add($"{prefix}{symbol}]");
			}
			else
			{
				cards.Add("[---]");
			}
		}
		return string.Join("", cards);
	}

	private string FormatRegisters(Player player, int currentRegister, bool programming, bool isCurrentPlayer)
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
				else if (programming && i == currentRegister)
				{
					prefix = "▼";
				}
			}

			var card = player.GetRegister(i);
			registers.Add($"{prefix}{(card == null ? "---" : GetCardSymbol(card))}{suffix}");
		}
		return $"[{string.Join("][", registers)}]";
	}

	private string FormatPlayerStatus(Player player)
	{
		var dir = GetDirectionSymbol(player.Robot.Direction);
		var status = player.Robot.IsAlive ? "❤️" : "💀";
		return $"({player.Robot.Position.X},{player.Robot.Position.Y}) {dir} {player.Robot.Damage} {status}";
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
