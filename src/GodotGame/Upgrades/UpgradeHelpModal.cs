using Godot;
using System.Linq;

namespace RoboRallyGame.Godot.Upgrades;

public enum UpgradeCategoryFilter
{
	All,
	Permanent,
	Temporary
}

public partial class UpgradeHelpModal : PanelContainer
{
	[Signal] public delegate void CloseRequestedEventHandler();

	private VBoxContainer _upgradesList;
	private Button _allButton;
	private Button _permanentButton;
	private Button _temporaryButton;
	private UpgradeCategoryFilter _currentFilter = UpgradeCategoryFilter.All;

	public override void _Ready()
	{
		_upgradesList = GetNode<VBoxContainer>("VBox/ScrollContainer/UpgradesList");
		_allButton = GetNode<Button>("VBox/FilterContainer/AllButton");
		_permanentButton = GetNode<Button>("VBox/FilterContainer/PermanentButton");
		_temporaryButton = GetNode<Button>("VBox/FilterContainer/TemporaryButton");

		_allButton.Pressed += () => SetFilter(UpgradeCategoryFilter.All);
		_permanentButton.Pressed += () => SetFilter(UpgradeCategoryFilter.Permanent);
		_temporaryButton.Pressed += () => SetFilter(UpgradeCategoryFilter.Temporary);

		var closeButton = GetNode<Button>("VBox/Header/CloseButton");
		closeButton.Pressed += () => Visible = false;

		PopulateUpgrades();
	}

	private void SetFilter(UpgradeCategoryFilter filter)
	{
		_currentFilter = filter;
		PopulateUpgrades();
	}

	private void PopulateUpgrades()
	{
		foreach (var child in _upgradesList.GetChildren())
		{
			child.QueueFree();
		}

		var upgrades = Core.Upgrades.UpgradeCatalog.AllUpgrades
			.Where(u => _currentFilter switch
			{
				UpgradeCategoryFilter.Permanent => u.Type == Core.Upgrades.UpgradeType.Permanent,
				UpgradeCategoryFilter.Temporary => u.Type == Core.Upgrades.UpgradeType.Temporary,
				_ => true
			})
			.ToList();

		foreach (var upgrade in upgrades)
		{
			var row = CreateUpgradeRow(upgrade);
			_upgradesList.AddChild(row);
		}
	}

	private static Control CreateUpgradeRow(Core.Upgrades.UpgradeDefinition upgrade)
	{
		var hbox = new HBoxContainer();

		var iconLabel = new Label { Text = GetUpgradeIcon(upgrade.Id) };
		iconLabel.CustomMinimumSize = new Vector2(40, 0);
		hbox.AddChild(iconLabel);

		var nameLabel = new Label { Text = upgrade.Name };
		nameLabel.CustomMinimumSize = new Vector2(150, 0);
		hbox.AddChild(nameLabel);

		var descLabel = new Label
		{
			Text = upgrade.Description,
			HorizontalAlignment = HorizontalAlignment.Left,
		};
		hbox.AddChild(descLabel);

		var typeLabel = new Label 
		{ 
			Text = upgrade.Type.ToString(),
			HorizontalAlignment = HorizontalAlignment.Right
		};
		typeLabel.CustomMinimumSize = new Vector2(80, 0);
		hbox.AddChild(typeLabel);

		return hbox;
	}

	private static string GetUpgradeIcon(string upgradeId)
	{
		return upgradeId switch
		{
			"double_barrel_laser" => "🔫",
			"rear_firing_laser" => "🔙",
			"brakes" => "🛑",
			"crab_legs" => "🦀",
			"gyroscopic_stabilizer" => "🔄",
			"high_power_laser" => "⚡",
			"mechanical_arm" => "🦾",
			"mini_howitzer" => "💥",
			"tractor_beam" => "🧲",
			"superior_archive" => "📚",
			"ablative_coat" => "🛡️",
			"abort_switch" => "🔴",
			"circuit_breaker" => "⚡",
			"conditional_program" => "🔀",
			"dual_processor" => "🔲",
			"extra_memory" => "🧠",
			"fire_control" => "🎯",
			"flywheel" => "⚙️",
			"fourth_gear" => "⚙️",
			"power_down_shield" => "🛡️",
			"pressor_beam" => "💨",
			"radio_control" => "📡",
			"ramming_gear" => "🚗",
			"recompile" => "🔄",
			"reverse_gear" => "◀️",
			"scrambler" => "📡",
			_ => "⚙️"
		};
	}
}
