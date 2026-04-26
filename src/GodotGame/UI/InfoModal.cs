using Godot;

namespace RoboRallyGame.Godot.UI;

public partial class InfoModal : PanelContainer
{
	public override void _Ready()
	{
		var closeBtn = GetNodeOrNull<Button>("VBox/CloseButton");
		if (closeBtn != null)
			closeBtn.Pressed += () => Visible = false;
	}
}
