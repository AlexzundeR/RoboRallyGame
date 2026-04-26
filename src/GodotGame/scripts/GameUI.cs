using Godot;
using System.Collections.Generic;
using System.Linq;
using AiTest.Godot.UI;

namespace AiTest.Godot;

public partial class GameUI : CanvasLayer
{
	[Signal] public delegate void GiveUpgradeEventHandler();
	[Signal] public delegate void GiveDamageEventHandler();

	public PackedScene PlayerPanelScene { get; set; }

	private TopPanel _topPanel = null!;
	private ActionLog _actionLog = null!;
	private DebugMenu _debugMenu = null!;
	private PlayerStatusModal _playerStatusModal = null!;
	private InfoModal _infoModal = null!;
	private WinModal _winModal = null!;

	private HBoxContainer _playerPanelsContainer = null!;
	private readonly List<PlayerPanel> _playerPanels = new();

	public override void _Ready()
	{
		_topPanel = GetNode<TopPanel>("UIRoot/MainContainer/MainHBox/LeftContent/TopSection/TopPanel");
		_topPanel.InfoPressed += OnInfoPressed;

		_actionLog = GetNode<ActionLog>("UIRoot/MainContainer/MainHBox/ActionLog");

		_debugMenu = GetNode<DebugMenu>("UIRoot/ModalContainer/ModalVBox/DebugMenu");
		_debugMenu.GiveUpgrade += () => EmitSignal(SignalName.GiveUpgrade);
		_debugMenu.GiveDamage += () => EmitSignal(SignalName.GiveDamage);
		_debugMenu.Visible = false;

		_playerStatusModal = GetNode<PlayerStatusModal>("UIRoot/ModalContainer/ModalVBox/PlayerStatusModal");
		_playerStatusModal.Visible = false;

		_infoModal = GetNode<InfoModal>("UIRoot/ModalContainer/ModalVBox/InfoModal");
		_infoModal.Visible = false;

		_winModal = GetNode<WinModal>("UIRoot/ModalContainer/ModalVBox/WinModal");
		_winModal.Visible = false;

		InitializePlayerPanels();
	}

	private void InitializePlayerPanels()
	{
		if (!IsInsideTree() || PlayerPanelScene == null) return;

		_playerPanelsContainer = GetNode<HBoxContainer>("UIRoot/MainContainer/MainHBox/LeftContent/BottomSection/BottomPanel/PlayerHBox");

		for (int i = 0; i < 2; i++)
		{
			var panel = PlayerPanelScene.Instantiate<PlayerPanel>();
			panel.RegisterClicked += OnRegisterClicked;
			panel.UpgradesClicked += OnUpgradesClicked;
			panel.LayoutMode = 2;
			panel.SizeFlagsHorizontal = (Control.SizeFlags)3;
			_playerPanelsContainer.AddChild(panel);
			_playerPanels.Add(panel);
		}
	}

	private void OnRegisterClicked(int registerIndex)
	{
		GD.Print($"Register clicked: {registerIndex}");
	}

	private void OnUpgradesClicked()
	{
		OpenPlayerStatusModal();
	}

	private void OnInfoPressed()
	{
		if (_infoModal != null)
			_infoModal.Visible = !_infoModal.Visible;
	}

	public void OpenDebugMenu()
	{
		if (_debugMenu != null)
			_debugMenu.Visible = true;
	}

	public void OpenPlayerStatusModal()
	{
		if (_playerStatusModal == null) return;
		_playerStatusModal.Visible = true;
	}

	public void CloseAllModals()
	{
		if (_infoModal != null) _infoModal.Visible = false;
		if (_winModal != null) _winModal.Visible = false;
		if (_debugMenu != null) _debugMenu.Visible = false;
		if (_playerStatusModal != null) _playerStatusModal.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.Escape)
			{
				if (_winModal?.Visible == true) return;
				CloseAllModals();
			}
			else if (keyEvent.Keycode == Key.I)
			{
				CloseAllModals();
				OpenPlayerStatusModal();
			}
			else if (keyEvent.Keycode == Key.U)
			{
				EmitSignal(SignalName.GiveUpgrade);
			}
			else if (keyEvent.Keycode == Key.D)
			{
				EmitSignal(SignalName.GiveDamage);
			}
		}
	}

	public void UpdateRound(int round)
	{
		_topPanel?.UpdateRound(round);
	}

	public void UpdatePhase(bool programming)
	{
		_topPanel?.UpdatePhase(programming);
	}

	public void UpdateRegisterPrompt(int currentRegister, int selectedRegister, string playerName)
	{
		_topPanel?.UpdateRegisterPrompt(currentRegister, selectedRegister, playerName);
	}

	public void UpdateExecutionPrompt(int currentRegister, string playerName)
	{
		_topPanel?.UpdateExecutionPrompt(currentRegister, playerName);
	}

	public string PriorityText
	{
		set
		{
			if (_topPanel != null)
				_topPanel.PriorityText = value;
		}
	}

	public void UpdatePlayerInfo(PlayerViewModel p1, PlayerViewModel p2, int currentRegister, bool programming, int currentPlayerIndex, int selectedRegister = 0)
	{
		if (!IsInsideTree()) return;

		if (_playerPanels.Count >= 2)
		{
			_playerPanels[0].Update(p1, currentPlayerIndex == 0, programming, currentRegister, selectedRegister);
			_playerPanels[1].Update(p2, currentPlayerIndex == 1, programming, currentRegister, selectedRegister);
		}
	}

	public void UpdateCheckpointProgress(int playerId, int checkpointsReached, int totalCheckpoints)
	{
		if (!IsInsideTree()) return;
		var panelIndex = playerId - 1;
		if (panelIndex >= 0 && panelIndex < _playerPanels.Count)
		{
			_playerPanels[panelIndex].UpdateCheckpoints(checkpointsReached, totalCheckpoints);
		}
	}

	public void ShowGameOver(IReadOnlyList<string> winnerNames, bool isTie)
	{
		if (_winModal == null) return;

		var message = isTie
			? $"TIE! {string.Join(" & ", winnerNames)}"
			: $"{winnerNames[0]} WINS!";

		_winModal.SetMessage(message);
		_winModal.Visible = true;
	}

	public void Log(string message)
	{
		_actionLog?.Log(message);
	}
}
