using Godot;
using System.Collections.Generic;
using AiTest.Core.Entities;
using AiTest.Core.Models;

namespace AiTest.Godot;

public partial class GameUI : CanvasLayer
{
    [Export] private Label? _roundLabel;
    [Export] private Label? _phaseLabel;
    [Export] private Label? _priorityLabel;
    [Export] private Label? _registerLabel;
    [Export] private Label? _helpLabel;
    [Export] private Button? _infoButton;
    [Export] private PanelContainer? _statusModal;
    [Export] private Button? _closeButton;

    [Export] private Label? _p1Header;
    [Export] private Label? _p1Hand;
    [Export] private Label? _p1Registers;
    [Export] private Label? _p1Status;
    [Export] private Label? _p2Header;
    [Export] private Label? _p2Hand;
    [Export] private Label? _p2Registers;
    [Export] private Label? _p2Status;

    private bool _modalVisible = false;

    public string PriorityText
    {
        set
        {
            if (_priorityLabel != null)
                _priorityLabel.Text = value;
        }
    }

    public override void _Ready()
    {
        if (_roundLabel == null)
        {
            CreateUI();
        }
        
        if (_infoButton != null)
            _infoButton.Pressed += OnInfoPressed;

        if (_closeButton != null)
            _closeButton.Pressed += OnInfoPressed;
    }

    private void CreateUI()
    {
        var topPanel = new PanelContainer
        {
            Name = "TopPanel",
            OffsetLeft = 10,
            OffsetTop = 10,
            OffsetRight = -10,
            OffsetBottom = 75
        };
        topPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        AddChild(topPanel);

        var topHBox = new HBoxContainer();
        topHBox.AddThemeConstantOverride("separation", 30);
        topPanel.AddChild(topHBox);

        var leftVBox = new VBoxContainer();
        leftVBox.AddThemeConstantOverride("separation", 2);
        topHBox.AddChild(leftVBox);

        var title = new Label { Text = "🤖 ROBO RALLY" };
        title.AddThemeFontSizeOverride("font_size", 20);
        leftVBox.AddChild(title);

        _roundLabel = new Label { Name = "RoundLabel", Text = "Round: 1" };
        leftVBox.AddChild(_roundLabel);

        _phaseLabel = new Label { Name = "PhaseLabel", Text = "PROGRAMMING" };
        _phaseLabel.AddThemeFontSizeOverride("font_size", 16);
        _phaseLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        leftVBox.AddChild(_phaseLabel);

        var middleVBox = new VBoxContainer();
        middleVBox.AddThemeConstantOverride("separation", 4);
        topHBox.AddChild(middleVBox);

        _priorityLabel = new Label { Name = "PriorityLabel", Text = "Priority: -" };
        _priorityLabel.AddThemeFontSizeOverride("font_size", 16);
        _priorityLabel.AddThemeColorOverride("font_color", Colors.Orange);
        middleVBox.AddChild(_priorityLabel);

        _registerLabel = new Label { Name = "RegisterLabel", Text = "Select register 1/5" };
        _registerLabel.AddThemeFontSizeOverride("font_size", 14);
        _registerLabel.AddThemeColorOverride("font_color", Colors.Cyan);
        middleVBox.AddChild(_registerLabel);

        var rightVBox = new VBoxContainer();
        rightVBox.AddThemeConstantOverride("separation", 4);
        topHBox.AddChild(rightVBox);

        _helpLabel = new Label { Text = "Press 1-9 to select" };
        _helpLabel.AddThemeColorOverride("font_color", Colors.Gray);
        rightVBox.AddChild(_helpLabel);

        _infoButton = new Button { Text = "ℹ️ Info" };
        _infoButton.Pressed += OnInfoPressed;
        topHBox.AddChild(_infoButton);

        var bottomPanel = new PanelContainer
        {
            Name = "BottomPanel"
        };
        bottomPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        bottomPanel.OffsetLeft = 10;
        bottomPanel.OffsetTop = -160;
        bottomPanel.OffsetRight = -10;
        bottomPanel.OffsetBottom = -10;
        AddChild(bottomPanel);

        var bottomHBox = new HBoxContainer();
        bottomHBox.AddThemeConstantOverride("separation", 40);
        bottomPanel.AddChild(bottomHBox);

        var p1VBox = new VBoxContainer();
        p1VBox.AddThemeConstantOverride("separation", 4);
        bottomHBox.AddChild(p1VBox);

        _p1Header = new Label { Text = "🔵 PLAYER 1" };
        _p1Header.AddThemeFontSizeOverride("font_size", 14);
        _p1Header.AddThemeColorOverride("font_color", Colors.DodgerBlue);
        p1VBox.AddChild(_p1Header);

        _p1Hand = new Label { Text = "Hand: [1][2][3][4][5][6][7][8][9]" };
        _p1Hand.AddThemeFontSizeOverride("font_size", 10);
        p1VBox.AddChild(_p1Hand);

        _p1Registers = new Label { Text = "Regs: [---][---][---][---][---]" };
        _p1Registers.AddThemeFontSizeOverride("font_size", 11);
        p1VBox.AddChild(_p1Registers);

        _p1Status = new Label { Text = "(0,0) ⬆ ❤️ 0" };
        _p1Status.AddThemeFontSizeOverride("font_size", 10);
        p1VBox.AddChild(_p1Status);

        var p2VBox = new VBoxContainer();
        p2VBox.AddThemeConstantOverride("separation", 4);
        bottomHBox.AddChild(p2VBox);

        _p2Header = new Label { Text = "🔴 PLAYER 2" };
        _p2Header.AddThemeFontSizeOverride("font_size", 14);
        _p2Header.AddThemeColorOverride("font_color", Colors.Red);
        p2VBox.AddChild(_p2Header);

        _p2Hand = new Label { Text = "Hand: [1][2][3][4][5][6][7][8][9]" };
        _p2Hand.AddThemeFontSizeOverride("font_size", 10);
        p2VBox.AddChild(_p2Hand);

        _p2Registers = new Label { Text = "Regs: [---][---][---][---][---]" };
        _p2Registers.AddThemeFontSizeOverride("font_size", 11);
        p2VBox.AddChild(_p2Registers);

        _p2Status = new Label { Text = "(0,0) ⬆ ❤️ 0" };
        _p2Status.AddThemeFontSizeOverride("font_size", 10);
        p2VBox.AddChild(_p2Status);

        _statusModal = new PanelContainer
        {
            Name = "StatusModal",
            Visible = false
        };
        _statusModal.SetAnchorsPreset(Control.LayoutPreset.Center);
        _statusModal.OffsetLeft = -200;
        _statusModal.OffsetTop = -150;
        _statusModal.OffsetRight = 200;
        _statusModal.OffsetBottom = 150;
        AddChild(_statusModal);

        var modalVBox = new VBoxContainer();
        modalVBox.AddThemeConstantOverride("separation", 15);
        _statusModal.AddChild(modalVBox);

        var modalTitle = new Label { Text = "📋 GAME INFO" };
        modalTitle.HorizontalAlignment = HorizontalAlignment.Center;
        modalVBox.AddChild(modalTitle);

        _closeButton = new Button { Text = "❌ Close" };
        _closeButton.Pressed += OnInfoPressed;
        modalVBox.AddChild(_closeButton);

        var controlsLabel = new Label
        {
            Text = "🎮 CONTROLS\n\n1-9: Select card for register\nSPACE: Confirm / Execute\nESC: Restart game",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        modalVBox.AddChild(controlsLabel);
    }

    private void OnInfoPressed()
    {
        _modalVisible = !_modalVisible;
        if (_statusModal != null)
            _statusModal.Visible = _modalVisible;
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

    public void UpdateRegisterPrompt(int currentRegister, string playerName)
    {
        if (_registerLabel == null) return;

        var nextReg = currentRegister < 5 ? currentRegister + 1 : 5;
        _registerLabel.Text = $"{playerName}: Select register {nextReg}/5";
    }

    public void UpdateExecutionPrompt(int currentRegister, string playerName)
    {
        if (_registerLabel == null) return;

        _registerLabel.Text = $"{playerName}: Executing register {currentRegister + 1}/5";
    }

    public void UpdatePlayerInfo(Player p1, Player p2, int currentRegister, bool programming, int currentPlayerIndex)
    {
        if (_p1Hand != null)
            _p1Hand.Text = $"Hand: {FormatHand(p1, currentPlayerIndex == 0 && programming)}";
        if (_p1Registers != null)
            _p1Registers.Text = $"Regs: {FormatRegisters(p1, currentRegister, programming, currentPlayerIndex == 0)}";
        if (_p1Status != null)
            _p1Status.Text = FormatPlayerStatus(p1);

        if (_p2Hand != null)
            _p2Hand.Text = $"Hand: {FormatHand(p2, currentPlayerIndex == 1 && programming)}";
        if (_p2Registers != null)
            _p2Registers.Text = $"Regs: {FormatRegisters(p2, currentRegister, programming, currentPlayerIndex == 1)}";
        if (_p2Status != null)
            _p2Status.Text = FormatPlayerStatus(p2);
    }

    private string FormatHand(Player player, bool isCurrentPlayer)
    {
        var cards = new List<string>();
        for (int i = 0; i < 9; i++)
        {
            if (i < player.Hand.Count)
            {
                var symbol = GetCardSymbol(player.Hand[i]);
                var prefix = isCurrentPlayer ? $"[{i + 1}:" : "[";
                cards.Add($"{prefix}{symbol}]");
            }
            else
            {
                cards.Add("[---]");
            }
        }
        return string.Join("", cards);
    }

    private string FormatRegisters(Player player, int currentRegister, bool programming, bool isCurrentPlayer)
    {
        var registers = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            var prefix = "";
            var suffix = "";

            if (isCurrentPlayer)
            {
                if (!programming && i == currentRegister)
                {
                    prefix = "▶";
                    suffix = "◀";
                }
                else if (programming && i == currentRegister)
                {
                    prefix = "▼";
                }
            }

            var card = player.GetRegister(i);
            registers.Add($"{prefix}{(card == null ? "---" : GetCardSymbol(card))}{suffix}");
        }
        return $"[{string.Join("][", registers)}]";
    }

    private string FormatPlayerStatus(Player player)
    {
        var dir = GetDirectionSymbol(player.Robot.Direction);
        var status = player.Robot.IsAlive ? "❤️" : "💀";
        return $"({player.Robot.Position.X},{player.Robot.Position.Y}) {dir} {player.Robot.Damage} {status}";
    }

    private string GetCardSymbol(ProgramCard card)
    {
        return card switch
        {
            MoveCard m => $"M{m.Spaces}",
            TurnCard t => t.IsLeft ? "↺" : "↻",
            UTurnCard => "⟲",
            _ => "?"
        };
    }

    private string GetDirectionSymbol(Direction direction)
    {
        return direction switch
        {
            Direction.North => "⬆",
            Direction.East => "➡",
            Direction.South => "⬇",
            Direction.West => "⬅",
            _ => "?"
        };
    }
}
