using Godot;
using System;

public partial class RegisterActionCard : PanelContainer
{
	// Экспортируемые свойства для настройки в редакторе
	[Export] public string CardSymbol { get; set; }
	[Export] public int CardPriority { get; set; } = 100;
	[Export] public int CardNumber { get; set; } = 1;
	[Export] public bool IsSelected { get; private set; }
	[Export] public bool IsFixed { get; private set; } = false;
	[Export] public Theme CardTheme { get; set;}

	private Label _numberLabel;
	private Label _symbolLabel;
	private Label _priorityLabel;
	private ColorRect _background;
	private Panel _border;

	private AnimationPlayer _animationPlayer;

	public override void _Ready()
	{
		_numberLabel = GetNode<Label>("GridContainer/Number");
		_symbolLabel = GetNode<Label>("GridContainer/Symbol");
		_priorityLabel = GetNode<Label>("GridContainer/Priority");
		_background = GetNode<ColorRect>("Background");
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer"); ;
		_border = GetNode<Panel>("Border");
		UpdateDisplay();
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion)
		{
			_animationPlayer.Play("card_hover_animation");
		}
		else if (@event is InputEventMouseButton && !@event.IsPressed())
		{
			_animationPlayer.PlayBackwards("card_hover_animation");
		}
	}

	private void UpdateDisplay()
	{
		if (_numberLabel == null)
		{
			return;
		}

		if (CardSymbol == null)
		{
			_background.Color = Colors.Black;
		}

		if (IsSelected)
		{
			_border.Visible = true;
		}
		else
		{
			_border.Visible = true;
		}
		if (IsFixed && IsSelected)
		{
			GD.Print("OOOOPS");
		}

		if (IsFixed)
		{
			_border.AddThemeStyleboxOverride("panel", CardTheme.GetStylebox("fixed", "Panel"));
		}
		else if (IsSelected)
		{
			var sb = CardTheme.GetStylebox("selected", "Panel");
			_border.AddThemeStyleboxOverride("panel", sb);
		}
		else
		{
			_border.AddThemeStyleboxOverride("panel", CardTheme.GetStylebox("panel", "Panel"));
		}
		

		if (CardSymbol == null) return;

		_symbolLabel.Text = $"{CardSymbol}";
		_numberLabel.Text = $"{CardNumber}";
		_priorityLabel.Text = $"{CardPriority}";
	}

	// Метод для обновления данных карточки

	internal void SetCardData(string symbol, int priority, int number)
	{
		CardSymbol = symbol;
		CardPriority = priority;
		CardNumber = number;
		UpdateDisplay();
	}

	internal void SetEmpty()
	{
		UpdateDisplay();
	}

	internal void SetSelected(bool isSelected)
	{
		IsSelected = isSelected;
	}

	internal void SetFixed(bool isFixed)
	{
		IsFixed = isFixed;
	}

}
