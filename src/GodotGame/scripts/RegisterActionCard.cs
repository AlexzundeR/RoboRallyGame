using Godot;
using System;

public partial class RegisterActionCard : PanelContainer
{
	public bool IsEmpty => CardSymbol == null;

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
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
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
		if (!IsInsideTree()) return;

		var styles = CardStyleFactory.Evaluate(CardTheme, IsEmpty, IsFixed, IsSelected);

		_background.Color = styles.BackgroundColor;
		_border.AddThemeStyleboxOverride("panel", styles.BorderStyle);

		if (CardSymbol != null)
		{
			_symbolLabel.Text = CardSymbol;
			_numberLabel.Text = CardNumber.ToString();
			_priorityLabel.Text = CardPriority.ToString();
		}
	}

	// Метод для обновления данных карточки

	internal void SetCardData(string symbol, int priority, int number)
	{
		if (CardSymbol != null)
		{
			//GD.Print($"[Card] UpdateDisplay: state={IsSelected}, Symbol={CardSymbol}");
		}
		CardSymbol = symbol;
		CardPriority = priority;
		CardNumber = number;
		UpdateDisplay();
	}

	internal void SetEmpty()
	{
		CardSymbol = null;
		CardPriority = 0;
		CardNumber = 0;
		UpdateDisplay();
	}

	internal void SetSelected(bool isSelected)
	{
		IsSelected = isSelected;
		UpdateDisplay();
	}

	internal void SetFixed(bool isFixed)
	{
		IsFixed = isFixed;
		UpdateDisplay();
	}

	public record CardStyles(StyleBox BorderStyle, Color BackgroundColor);

	public static class CardStyleFactory
	{
		private const string ThemeType = "Card";

		public static CardStyles Evaluate(Theme theme, bool isEmpty, bool isFixed, bool isSelected)
		{
			string styleName = "normal";
			if (isFixed && isSelected)
				styleName = "selected_fixed";
			else if (isFixed)
				styleName = "fixed";
			else if (isSelected)
				styleName = "selected";

			return new CardStyles(
				theme.GetStylebox(styleName, ThemeType),
				isEmpty
					? theme.GetColor("empty_bg", ThemeType)
					: theme.GetColor("normal_bg", ThemeType)
			);
		}
	}
}
