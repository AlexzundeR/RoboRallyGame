using Godot;

namespace RoboRallyGame.Godot.UI;

public partial class WinModal : PanelContainer
{
	private Label? _winLabel;

	public override void _Ready()
	{
		_winLabel = GetNode<Label>("VBox/WinLabel");
	}

	public void SetMessage(string message)
	{
		if (_winLabel != null)
			_winLabel.Text = message;
	}
}
