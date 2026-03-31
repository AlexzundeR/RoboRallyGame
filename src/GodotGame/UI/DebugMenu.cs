using Godot;

namespace AiTest.Godot.UI;

public partial class DebugMenu : PanelContainer
{
	[Signal] public delegate void GiveUpgradeEventHandler();
	[Signal] public delegate void GiveDamageEventHandler();

	public override void _Ready()
	{
		var giveUpgradeBtn = GetNodeOrNull<Button>("VBox/GiveUpgradeButton");
		var giveDamageBtn = GetNodeOrNull<Button>("VBox/GiveDamageButton");
		var closeBtn = GetNodeOrNull<Button>("VBox/CloseButton");

		if (giveUpgradeBtn != null)
			giveUpgradeBtn.Pressed += () => EmitSignal(SignalName.GiveUpgrade);
		if (giveDamageBtn != null)
			giveDamageBtn.Pressed += () => EmitSignal(SignalName.GiveDamage);
		if (closeBtn != null)
			closeBtn.Pressed += () => Visible = false;
	}
}
