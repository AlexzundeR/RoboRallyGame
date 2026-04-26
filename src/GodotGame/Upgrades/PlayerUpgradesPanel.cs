using Godot;
using System.Collections.Generic;
using System.Linq;
using RoboRallyGame.Core.Entities;
using RoboRallyGame.Core.Upgrades;

namespace RoboRallyGame.Godot.Upgrades;

public partial class PlayerUpgradesPanel : PanelContainer
{
	[Signal] public delegate void UpgradeClickedEventHandler(string upgradeId);
	[Signal] public delegate void HelpRequestedEventHandler();

	private VBoxContainer _vBox;
	private Label _permanentCount;
	private HBoxContainer _permanentCardsContainer;
	private Button _helpButton;

	private PackedScene _upgradeCardScene;
	private readonly List<Control> _upgradeCards = new();

	public override void _Ready()
	{
		_vBox = GetNode<VBoxContainer>("VBox");
		
		var permanentSection = _vBox.GetNode<VBoxContainer>("PermanentSection");
		_permanentCount = permanentSection.GetNode<Label>("SectionHeader/Count");
		_permanentCardsContainer = permanentSection.GetNode<HBoxContainer>("CardsScroll/CardsContainer");
		
		_helpButton = _vBox.GetNode<Button>("Header/HelpButton");
		_helpButton.Pressed += OnHelpPressed;

		_upgradeCardScene = GD.Load<PackedScene>("res://Upgrades/UpgradeCard.tscn");
	}

	public void Update(Player player)
	{
		UpdateUpgrades(player.AllUpgrades.ToList());
	}

	private void UpdateUpgrades(List<UpgradeEffect> upgrades)
	{
		foreach (var card in _upgradeCards)
		{
			card.QueueFree();
		}
		_upgradeCards.Clear();

		foreach (var upgrade in upgrades)
		{
			var card = CreateUpgradeCard(upgrade);
			_permanentCardsContainer.AddChild(card);
			_upgradeCards.Add(card);
		}

		_permanentCount.Text = $"({upgrades.Count})";
	}

	private Control CreateUpgradeCard(UpgradeEffect upgrade)
	{
		var definition = UpgradeCatalog.Get(upgrade.Name.ToLowerInvariant().Replace(" ", "_")) 
			?? UpgradeCatalog.AllUpgrades.FirstOrDefault(u => u.Name == upgrade.Name);
		
		var card = _upgradeCardScene.Instantiate<UpgradeCard>();
		card.SetUpgrade(new UpgradeCardData
		{
			Id = definition?.Id ?? upgrade.Name,
			Name = upgrade.Name,
			Icon = GetUpgradeIcon(upgrade.Name),
			Description = definition?.Description ?? "",
			MaxCharges = definition?.MaxUses ?? -1
		});
		
		card.UpgradeClicked += OnUpgradeClicked;
		return card;
	}

	private static string GetUpgradeIcon(string upgradeName)
	{
		return upgradeName.ToLowerInvariant() switch
		{
			"double-barreled laser" => "🔫",
			"rear-firing laser" => "🔙",
			"brakes" => "🛑",
			"crab legs" => "🦀",
			"gyroscopic stabilizer" => "🔄",
			"high-power laser" => "⚡",
			"mechanical arm" => "🦾",
			"mini howitzer" => "💥",
			"tractor beam" => "🧲",
			"superior archive" => "📚",
			_ => "⚙️"
		};
	}

	private void OnUpgradeClicked(string upgradeId)
	{
		EmitSignal(SignalName.UpgradeClicked, upgradeId);
	}

	private void OnHelpPressed()
	{
		EmitSignal(SignalName.HelpRequested);
	}

	public void SetHelpButtonVisible(bool visible)
	{
		_helpButton.Visible = visible;
	}
}
