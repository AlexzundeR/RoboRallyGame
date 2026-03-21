using Godot;
using System.Collections.Generic;
using AiTest.Core.Entities;

namespace AiTest.Godot;

public partial class GameUI : CanvasLayer
{
	private Label _roundLabel;
	private Label _phaseLabel;
	private Label _priorityLabel;
	private Label _registerLabel;
	private Label _actionPrompt;
	private Label _helpLabel;
	private Button _infoButton;
	private PanelContainer _statusModal;

	private HBoxContainer _playerPanelsContainer;
	private readonly List<PlayerPanel> _playerPanels = new();

	private bool _modalVisible = false;

	public PackedScene PlayerPanelScene { get; set; }

	public string PriorityText
	{
		set
		{
			if (!IsInsideTree()) return;
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
		_playerPanelsContainer = GetNode<HBoxContainer>("BottomHBox/BottomPanel/HBox");

		InitializePlayerPanels();
		_infoButton.Pressed += OnInfoPressed;
	}

	private void InitializePlayerPanels()
	{
		if (!IsInsideTree()) return;
		if (PlayerPanelScene == null) return;

		for (int i = 0; i < 2; i++)
		{
			var panel = PlayerPanelScene.Instantiate<PlayerPanel>();
			panel.RegisterClicked += OnRegisterClicked;
			_playerPanelsContainer.AddChild(panel);
			_playerPanels.Add(panel);
		}
	}

	private void OnRegisterClicked(int registerIndex)
	{
		GD.Print($"Register clicked: {registerIndex}");
	}

	private void OnInfoPressed()
	{
		if (!IsInsideTree()) return;
		_modalVisible = !_modalVisible;
		_statusModal.Visible = _modalVisible;
	}

	public void UpdateRound(int round)
	{
		if (!IsInsideTree()) return;
		_roundLabel.Text = $"Round: {round}";
	}

	public void UpdatePhase(bool programming)
	{
		if (!IsInsideTree()) return;
		_phaseLabel.Text = programming ? "PROGRAMMING" : "EXECUTION";
		_phaseLabel.AddThemeColorOverride("font_color", programming ? Colors.Yellow : Colors.Green);
	}

	public void UpdateRegisterPrompt(int currentRegister, int selectedRegister, string playerName)
	{
		if (!IsInsideTree()) return;
		_registerLabel.Text = $"{playerName}: Register [{selectedRegister + 1}]/5";
		_actionPrompt.Text = $"SELECT {playerName.ToUpper()}: <- -> SELECT REGISTER | 1-9 SELECT CARD";
	}

	public void UpdateExecutionPrompt(int currentRegister, string playerName)
	{
		if (!IsInsideTree()) return;
		_registerLabel.Text = $"{playerName}: Executing register {currentRegister + 1}/5";
		_actionPrompt.Text = $"EXECUTING {playerName.ToUpper()}: REGISTER {currentRegister + 1} (PRESS SPACE)";
	}

	public void UpdatePlayerInfo(Player p1, Player p2, int currentRegister, bool programming, int currentPlayerIndex, int selectedRegister = 0)
	{
		if (!IsInsideTree()) return;
		if (_playerPanels.Count >= 2)
		{
			_playerPanels[0].Update(p1, currentPlayerIndex == 0, programming, currentRegister, selectedRegister);
			_playerPanels[1].Update(p2, currentPlayerIndex == 1, programming, currentRegister, selectedRegister);
		}
	}
}
