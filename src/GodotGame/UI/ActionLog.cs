using Godot;

namespace AiTest.Godot.UI;

public partial class ActionLog : PanelContainer
{
	private RichTextLabel? _logLabel;

	public override void _Ready()
	{
		_logLabel = GetNode<RichTextLabel>("VBox/ScrollContainer/LogLabel");
	}

	public void Log(string message)
	{
		if (_logLabel != null)
		{
			_logLabel.AppendText(message + "\n");
		}
	}

	public void Clear()
	{
		if (_logLabel != null)
		{
			_logLabel.Clear();
			_logLabel.AppendText("Ready...\n");
		}
	}

	public RichTextLabel? GetLogLabel() => _logLabel;
}
