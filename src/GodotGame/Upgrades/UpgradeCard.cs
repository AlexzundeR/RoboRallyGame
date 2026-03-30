using Godot;
using AiTest.Core.Upgrades;

namespace AiTest.Godot.Upgrades;

public partial class UpgradeCard : PanelContainer
{
	[Signal] public delegate void UpgradeClickedEventHandler(string upgradeId);

	private Label _icon;
	private Label _name;
	private Label _charges;
	private string _upgradeId = "";
	private int _currentCharges = -1;
	private bool _isActive = true;

	public override void _Ready()
	{
		_icon = GetNode<Label>("VBox/Icon");
		_name = GetNode<Label>("VBox/Name");
		_charges = GetNode<Label>("VBox/Charges");
	}

	public void SetUpgrade(UpgradeCardData data)
	{
		_upgradeId = data.Id;
		_icon.Text = data.Icon;
		_name.Text = data.Name;
		_currentCharges = data.MaxCharges;
		UpdateChargesDisplay();
	}

	public void SetCharges(int charges)
	{
		_currentCharges = charges;
		UpdateChargesDisplay();
	}

	public void SetActive(bool active)
	{
		_isActive = active;
		Modulate = active ? Colors.White : new Color(0.5f, 0.5f, 0.5f, 1);
	}

	public void SetSelected(bool selected)
	{
		if (selected)
		{
			AddThemeColorOverride("border_color", Colors.Yellow);
			AddThemeConstantOverride("border_width_left", 2);
			AddThemeConstantOverride("border_width_top", 2);
			AddThemeConstantOverride("border_width_right", 2);
			AddThemeConstantOverride("border_width_bottom", 2);
		}
		else
		{
			AddThemeConstantOverride("border_width_left", 0);
			AddThemeConstantOverride("border_width_top", 0);
			AddThemeConstantOverride("border_width_right", 0);
			AddThemeConstantOverride("border_width_bottom", 0);
		}
	}

	private void UpdateChargesDisplay()
	{
		if (_currentCharges < 0)
		{
			_charges.Text = "∞";
		}
		else
		{
			_charges.Text = _currentCharges.ToString();
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
		{
			EmitSignal(SignalName.UpgradeClicked, _upgradeId);
		}
	}

	public override string ToString() => _upgradeId;
}

public record UpgradeCardData
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public required string Icon { get; init; }
	public int MaxCharges { get; init; } = -1;
	public string Description { get; init; } = "";
}
