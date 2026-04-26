using Godot;
using System.Collections.Generic;
using System.Linq;
using RoboRallyGame.Godot;

namespace RoboRallyGame.Godot.UI;

public partial class PlayerStatusModal : PanelContainer
{
	[Signal] public delegate void PlayerChangedEventHandler(int playerIndex);

	private Label _playerName;
	private Label _statsLabel;
	private VBoxContainer _upgradesList;
	private Label _noUpgradesLabel;
	private Button _prevButton;
	private Button _nextButton;

	private List<PlayerViewModel> _players = new();
	private int _currentIndex = 0;

	public override void _Ready()
	{
		_playerName = GetNode<Label>("VBox/PlayerName");
		_statsLabel = GetNode<Label>("VBox/StatsLabel");
		_upgradesList = GetNode<VBoxContainer>("VBox/UpgradesScroll/UpgradesList");
		_noUpgradesLabel = GetNode<Label>("VBox/UpgradesScroll/UpgradesList/NoUpgrades");
		_prevButton = GetNode<Button>("VBox/ButtonsHBox/PrevButton");
		_nextButton = GetNode<Button>("VBox/ButtonsHBox/NextButton");

		var closeBtn = GetNode<Button>("VBox/ButtonsHBox/CloseButton");
		if (closeBtn != null)
			closeBtn.Pressed += () => Visible = false;

		if (_prevButton != null)
			_prevButton.Pressed += OnPrevPressed;
		if (_nextButton != null)
			_nextButton.Pressed += OnNextPressed;
	}

	public void SetPlayers(IEnumerable<PlayerViewModel> players)
	{
		_players = players.ToList();
		_currentIndex = 0;
		UpdateDisplay();
	}

	private void OnPrevPressed()
	{
		if (_players.Count == 0) return;
		_currentIndex = (_currentIndex - 1 + _players.Count) % _players.Count;
		UpdateDisplay();
		EmitSignal(SignalName.PlayerChanged, _currentIndex);
	}

	private void OnNextPressed()
	{
		if (_players.Count == 0) return;
		_currentIndex = (_currentIndex + 1) % _players.Count;
		UpdateDisplay();
		EmitSignal(SignalName.PlayerChanged, _currentIndex);
	}

	private void UpdateDisplay()
	{
		if (_players.Count == 0) return;

		var player = _players[_currentIndex];

		if (_playerName != null)
			_playerName.Text = player.Name;

		if (_statsLabel != null)
		{
			_statsLabel.Text = $"HP: {player.LifeTokens}/3 | DMG: {player.Damage} | Stars: {player.CheckpointsReached}";
		}

		if (_prevButton != null)
			_prevButton.Text = _players.Count > 1 ? "<" : "";
		if (_nextButton != null)
			_nextButton.Text = _players.Count > 1 ? ">" : "";

		RefreshUpgrades(player);
	}

	private void RefreshUpgrades(PlayerViewModel player)
	{
		if (_upgradesList == null || _noUpgradesLabel == null) return;

		foreach (var child in _upgradesList.GetChildren())
		{
			if (child.Name != "NoUpgrades")
				child.QueueFree();
		}

		var upgrades = player.AllUpgrades.ToList();
		_noUpgradesLabel.Visible = upgrades.Count == 0;

		foreach (var upgrade in upgrades)
		{
			var label = new Label
			{
				Text = $"[b]{upgrade}[/b]",
				HorizontalAlignment = HorizontalAlignment.Left,
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			_upgradesList.AddChild(label);
		}
	}

	public PlayerViewModel? GetCurrentPlayer()
	{
		return _players.Count > 0 ? _players[_currentIndex] : null;
	}
}