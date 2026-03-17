using Godot;
using System.Collections.Generic;
using AiTest.Core.Entities;
using AiTest.Core.Models;

namespace AiTest.Godot;

public partial class GameUI : CanvasLayer
{
	[Export] private Label? _roundLabel;
	[Export] private Label? _phaseLabel;
	[Export] private Label? _priorityLabel;
	[Export] private Label? _registerLabel;
	[Export] private Label? _helpLabel;
	[Export] private Button? _infoButton;
	[Export] private PanelContainer? _statusModal;
	[Export] private Button? _closeButton;

	[Export] private Label? _p1Header;
	[Export] private Label? _p1Hand;
	[Export] private Label? _p1Registers;
	[Export] private Label? _p1Status;
	[Export] private Label? _p2Header;
	[Export] private Label? _p2Hand;
	[Export] private Label? _p2Registers;
	[Export] private Label? _p2Status;

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
