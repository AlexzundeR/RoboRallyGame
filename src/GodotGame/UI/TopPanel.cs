using Godot;

namespace RoboRallyGame.Godot.UI;

public partial class TopPanel : PanelContainer
{
	[Signal] public delegate void InfoPressedEventHandler();

	private Label? _roundLabel;
	private Label? _phaseLabel;
	private Label? _priorityLabel;
	private Label? _registerLabel;
	private Label? _actionPrompt;
	private Label? _helpLabel;
	private Button? _infoButton;

	public override void _Ready()
	{
		_roundLabel = GetNode<Label>("HBox/LeftPanel/RoundLabel");
		_phaseLabel = GetNode<Label>("HBox/LeftPanel/PhaseLabel");
		_priorityLabel = GetNode<Label>("HBox/MiddlePanel/PriorityLabel");
		_registerLabel = GetNode<Label>("HBox/MiddlePanel/RegisterLabel");
		_actionPrompt = GetNode<Label>("HBox/MiddlePanel/ActionPrompt");
		_helpLabel = GetNode<Label>("HBox/RightPanel/HelpLabel");
		_infoButton = GetNode<Button>("HBox/InfoButton");

		if (_infoButton != null)
			_infoButton.Pressed += () => EmitSignal(SignalName.InfoPressed);
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

	public void UpdateRegisterPrompt(int currentRegister, int selectedRegister, string playerName)
	{
		if (_registerLabel != null)
			_registerLabel.Text = $"{playerName}: Register [{selectedRegister + 1}]/5";
		if (_actionPrompt != null)
			_actionPrompt.Text = $"SELECT {playerName.ToUpper()}: <- -> SELECT REGISTER | 1-9 SELECT CARD";
	}

	public void UpdateExecutionPrompt(int currentRegister, string playerName)
	{
		if (_registerLabel != null)
			_registerLabel.Text = $"{playerName}: Executing register {currentRegister + 1}/5";
		if (_actionPrompt != null)
			_actionPrompt.Text = $"EXECUTING {playerName.ToUpper()}: REGISTER {currentRegister + 1} (PRESS SPACE)";
	}

	public string PriorityText
	{
		set
		{
			if (_priorityLabel != null)
				_priorityLabel.Text = value;
		}
	}
}
