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
    private Label? _p1RegistersLabel;
    private Label? _p1StatusLabel;
    private Label? _p2RegistersLabel;
    private Label? _p2StatusLabel;

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
    }

    private void CreateTopPanel()
    {
        var topPanel = new PanelContainer
        {
            Name = "TopPanel",
            OffsetLeft = 10,
            OffsetTop = 10,
            OffsetRight = -10,
            OffsetBottom = 80
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
    }

    private VBoxContainer CreateLeftPanel()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 4);

        var title = new Label
        {
            Text = "🤖 ROBO RALLY"
        };
        title.AddThemeFontSizeOverride("font_size", 24);
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
            Text = "Phase: PROGRAMMING"
        };
        _phaseLabel.AddThemeFontSizeOverride("font_size", 18);
        _phaseLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        vbox.AddChild(_phaseLabel);
        _labels["phase"] = _phaseLabel;

        return vbox;
    }

    private VBoxContainer CreateMiddlePanel()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 4);

        var cardSelectionLabel = new Label
        {
            Text = "🎴 SELECT CARDS FOR REGISTERS"
        };
        cardSelectionLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(cardSelectionLabel);

        var cardContainer = new HBoxContainer();
        cardContainer.AddThemeConstantOverride("separation", 8);
        vbox.AddChild(cardContainer);

        for (int i = 0; i < 9; i++)
        {
            var cardSlot = new TextureRect
            {
                CustomMinimumSize = new Vector2(50, 60),
                Modulate = new Color(0.7f, 0.7f, 0.7f)
            };
            cardContainer.AddChild(cardSlot);
        }

        return vbox;
    }

    private VBoxContainer CreateRightPanel()
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

        var helpLabel = new Label
        {
            Text = "1-5: Set register | SPACE: Execute"
        };
        helpLabel.AddThemeColorOverride("font_color", Colors.Gray);
        vbox.AddChild(helpLabel);

        _registerLabel = new Label
        {
            Name = "RegisterLabel",
            Text = "Select card for register 1/5 (keys 1-9)"
        };
        _registerLabel.AddThemeFontSizeOverride("font_size", 14);
        _registerLabel.AddThemeColorOverride("font_color", Colors.Cyan);
        vbox.AddChild(_registerLabel);
        _labels["register"] = _registerLabel;

        return vbox;
    }

    private void CreateBottomPanel()
    {
        var bottomPanel = new PanelContainer
        {
            Name = "BottomPanel",
            OffsetLeft = 10,
            OffsetTop = -90,
            OffsetRight = -10,
            OffsetBottom = -10
        };
        bottomPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        AddChild(bottomPanel);

        var bottomHBox = new HBoxContainer();
        bottomHBox.AddThemeConstantOverride("separation", 40);
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
        header.AddThemeFontSizeOverride("font_size", 16);
        header.AddThemeColorOverride("font_color", color);
        vbox.AddChild(header);

        _p1RegistersLabel = new Label
        {
            Name = $"{prefix}_registers",
            Text = "Registers: [ - ][ - ][ - ][ - ][ - ]"
        };
        vbox.AddChild(_p1RegistersLabel);
        _labels[$"{prefix}_registers"] = _p1RegistersLabel;

        _p1StatusLabel = new Label
        {
            Name = $"{prefix}_status",
            Text = "Pos: (0,0) ⬆ | ❤️ 0"
        };
        vbox.AddChild(_p1StatusLabel);
        _labels[$"{prefix}_status"] = _p1StatusLabel;

        if (prefix == "p1")
        {
            _p1RegistersLabel = _labels[$"{prefix}_registers"];
            _p1StatusLabel = _labels[$"{prefix}_status"];
        }
        else
        {
            _p2RegistersLabel = _labels[$"{prefix}_registers"];
            _p2StatusLabel = _labels[$"{prefix}_status"];
        }

        return vbox;
    }

    public void UpdateRound(int round) => _roundLabel!.Text = $"Round: {round}";

    public void UpdatePhase(bool programming)
    {
        _phaseLabel!.Text = programming ? "Phase: PROGRAMMING" : "Phase: EXECUTION";
        _phaseLabel.AddThemeColorOverride("font_color", programming ? Colors.Yellow : Colors.Green);
    }

    public void UpdateRegisterPrompt(int currentRegister, string playerName)
    {
        var nextReg = currentRegister < 5 ? currentRegister : 0;
        _registerLabel!.Text = $"{playerName}: Select card for register {nextReg + 1}/5 (keys 1-9)";
    }

    public void UpdateExecutionPrompt(int currentRegister, string playerName)
    {
        _registerLabel!.Text = $"{playerName}: Executing register {currentRegister + 1}/5";
    }

    public void UpdatePlayerInfo(Player p1, Player p2, int currentRegister, bool programming, int currentPlayerIndex)
    {
        _p1RegistersLabel!.Text = $"P1: {FormatRegisters(p1, currentRegister, programming, currentPlayerIndex == 0)}";
        _p1StatusLabel!.Text = FormatPlayerStatus(p1);

        _p2RegistersLabel!.Text = $"P2: {FormatRegisters(p2, currentRegister, programming, currentPlayerIndex == 1)}";
        _p2StatusLabel!.Text = FormatPlayerStatus(p2);
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
            registers.Add(card == null ? $"{prefix}---{suffix}" : $"{prefix}{GetCardSymbol(card)}{suffix}");
        }
        return $"[{string.Join("][", registers)}]";
    }

    private string FormatPlayerStatus(Player player)
    {
        var dir = GetDirectionSymbol(player.Robot.Direction);
        var status = player.Robot.IsAlive ? "❤️" : "💀";
        return $"Pos: ({player.Robot.Position.X},{player.Robot.Position.Y}) {dir} | {player.Robot.Damage} {status}";
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
