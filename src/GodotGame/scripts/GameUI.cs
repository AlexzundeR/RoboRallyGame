using Godot;
using System.Collections.Generic;
using AiTest.Core.Entities;
using AiTest.Core.Models;

namespace AiTest.Godot;

public partial class GameUI : CanvasLayer
{
    private readonly Dictionary<string, Label> _labels = new();
    private Label? _roundLabel;
    private Label? _phaseLabel;
    private Label? _priorityLabel;
    private Label? _registerLabel;
    private Label? _p1HandLabel;
    private Label? _p1RegistersLabel;
    private Label? _p1StatusLabel;
    private Label? _p2HandLabel;
    private Label? _p2RegistersLabel;
    private Label? _p2StatusLabel;
    private PanelContainer? _statusModal;
    private bool _modalVisible = false;

    public string RegisterText
    {
        set => _registerLabel!.Text = value;
    }

    public string PriorityText
    {
        set => _priorityLabel!.Text = value;
    }

    public override void _Ready()
    {
        CreateTopPanel();
        CreateBottomPanel();
        CreateStatusModal();
    }

    private void CreateTopPanel()
    {
        var topPanel = new PanelContainer
        {
            Name = "TopPanel",
            OffsetLeft = 10,
            OffsetTop = 10,
            OffsetRight = -10,
            OffsetBottom = 70
        };
        topPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        AddChild(topPanel);

        var topHBox = new HBoxContainer();
        topHBox.AddThemeConstantOverride("separation", 20);
        topPanel.AddChild(topHBox);

        var leftVBox = CreateLeftPanel();
        topHBox.AddChild(leftVBox);

        var middleVBox = CreateMiddlePanel();
        topHBox.AddChild(middleVBox);

        var rightVBox = CreateRightPanel();
        topHBox.AddChild(rightVBox);

        var infoButton = new Button
        {
            Text = "ℹ️ Info"
        };
        infoButton.Pressed += () => ToggleModal();
        topHBox.AddChild(infoButton);
    }

    private void CreateStatusModal()
    {
        _statusModal = new PanelContainer
        {
            Name = "StatusModal",
            Visible = false,
            OffsetLeft = 200,
            OffsetTop = 100,
            OffsetRight = -200,
            OffsetBottom = -100
        };
        _statusModal.SetAnchorsPreset(Control.LayoutPreset.Center);
        AddChild(_statusModal);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        _statusModal.AddChild(vbox);

        var title = new Label
        {
            Text = "📊 GAME INFO",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(title);

        var closeButton = new Button
        {
            Text = "Close"
        };
        closeButton.Pressed += () => ToggleModal();
        vbox.AddChild(closeButton);

        var controlsLabel = new Label
        {
            Text = "CONTROLS:\n" +
                   "1-9: Select card for register\n" +
                   "SPACE: Confirm selection / Execute register\n" +
                   "ESC: Restart game",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        vbox.AddChild(controlsLabel);
    }

    private void ToggleModal()
    {
        _modalVisible = !_modalVisible;
        _statusModal!.Visible = _modalVisible;
    }

    private VBoxContainer CreateLeftPanel()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 2);

        var title = new Label
        {
            Text = "🤖 ROBO RALLY"
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(title);

        _roundLabel = new Label
        {
            Name = "RoundLabel",
            Text = "Round: 1"
        };
        vbox.AddChild(_roundLabel);
        _labels["round"] = _roundLabel;

        _phaseLabel = new Label
        {
            Name = "PhaseLabel",
            Text = "PROGRAMMING"
        };
        _phaseLabel.AddThemeFontSizeOverride("font_size", 16);
        _phaseLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        vbox.AddChild(_phaseLabel);
        _labels["phase"] = _phaseLabel;

        return vbox;
    }

    private VBoxContainer CreateMiddlePanel()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 4);

        _priorityLabel = new Label
        {
            Name = "PriorityLabel",
            Text = "Priority: -"
        };
        _priorityLabel.AddThemeFontSizeOverride("font_size", 16);
        _priorityLabel.AddThemeColorOverride("font_color", Colors.Orange);
        vbox.AddChild(_priorityLabel);
        _labels["priority"] = _priorityLabel;

        _registerLabel = new Label
        {
            Name = "RegisterLabel",
            Text = "Select card for register 1/5"
        };
        _registerLabel.AddThemeFontSizeOverride("font_size", 14);
        _registerLabel.AddThemeColorOverride("font_color", Colors.Cyan);
        vbox.AddChild(_registerLabel);
        _labels["register"] = _registerLabel;

        return vbox;
    }

    private VBoxContainer CreateRightPanel()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 4);

        var helpLabel = new Label
        {
            Text = "Press 1-9 to select"
        };
        helpLabel.AddThemeColorOverride("font_color", Colors.Gray);
        vbox.AddChild(helpLabel);

        return vbox;
    }

    private void CreateBottomPanel()
    {
        var bottomPanel = new PanelContainer
        {
            Name = "BottomPanel",
            OffsetLeft = 10,
            OffsetTop = -140,
            OffsetRight = -10,
            OffsetBottom = -10
        };
        bottomPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        AddChild(bottomPanel);

        var bottomHBox = new HBoxContainer();
        bottomHBox.AddThemeConstantOverride("separation", 30);
        bottomPanel.AddChild(bottomHBox);

        var p1VBox = CreatePlayerPanel("🔵 PLAYER 1", Colors.DodgerBlue, "p1");
        bottomHBox.AddChild(p1VBox);

        var p2VBox = CreatePlayerPanel("🔴 PLAYER 2", Colors.Red, "p2");
        bottomHBox.AddChild(p2VBox);
    }

    private VBoxContainer CreatePlayerPanel(string title, Color color, string prefix)
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 2);

        var header = new Label
        {
            Text = title
        };
        header.AddThemeFontSizeOverride("font_size", 14);
        header.AddThemeColorOverride("font_color", color);
        vbox.AddChild(header);

        var handLabel = new Label
        {
            Name = $"{prefix}_hand",
            Text = "Hand: [1][2][3][4][5][6][7][8][9]"
        };
        handLabel.AddThemeFontSizeOverride("font_size", 10);
        vbox.AddChild(handLabel);
        _labels[$"{prefix}_hand"] = handLabel;

        if (prefix == "p1")
            _p1HandLabel = handLabel;
        else
            _p2HandLabel = handLabel;

        var registersLabel = new Label
        {
            Name = $"{prefix}_registers",
            Text = "Regs: [---][---][---][---][---]"
        };
        registersLabel.AddThemeFontSizeOverride("font_size", 11);
        vbox.AddChild(registersLabel);
        _labels[$"{prefix}_registers"] = registersLabel;

        if (prefix == "p1")
            _p1RegistersLabel = registersLabel;
        else
            _p2RegistersLabel = registersLabel;

        var statusLabel = new Label
        {
            Name = $"{prefix}_status",
            Text = "Pos: (0,0) ⬆ | ❤️ 0"
        };
        statusLabel.AddThemeFontSizeOverride("font_size", 10);
        vbox.AddChild(statusLabel);
        _labels[$"{prefix}_status"] = statusLabel;

        if (prefix == "p1")
            _p1StatusLabel = statusLabel;
        else
            _p2StatusLabel = statusLabel;

        return vbox;
    }

    public void UpdateRound(int round) => _roundLabel!.Text = $"Round: {round}";

    public void UpdatePhase(bool programming)
    {
        _phaseLabel!.Text = programming ? "PROGRAMMING" : "EXECUTION";
        _phaseLabel.AddThemeColorOverride("font_color", programming ? Colors.Yellow : Colors.Green);
    }

    public void UpdateRegisterPrompt(int currentRegister, string playerName)
    {
        var nextReg = currentRegister < 5 ? currentRegister + 1 : 5;
        _registerLabel!.Text = $"{playerName}: Select register {nextReg}/5";
    }

    public void UpdateExecutionPrompt(int currentRegister, string playerName)
    {
        _registerLabel!.Text = $"{playerName}: Executing register {currentRegister + 1}/5";
    }

    public void UpdatePlayerInfo(Player p1, Player p2, int currentRegister, bool programming, int currentPlayerIndex)
    {
        _p1HandLabel!.Text = $"Hand: {FormatHand(p1, currentPlayerIndex == 0 && programming)}";
        _p1RegistersLabel!.Text = $"Regs: {FormatRegisters(p1, currentRegister, programming, currentPlayerIndex == 0)}";
        _p1StatusLabel!.Text = FormatPlayerStatus(p1);

        _p2HandLabel!.Text = $"Hand: {FormatHand(p2, currentPlayerIndex == 1 && programming)}";
        _p2RegistersLabel!.Text = $"Regs: {FormatRegisters(p2, currentRegister, programming, currentPlayerIndex == 1)}";
        _p2StatusLabel!.Text = FormatPlayerStatus(p2);
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
                var suffix = "]";
                cards.Add($"{prefix}{symbol}{suffix}");
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
