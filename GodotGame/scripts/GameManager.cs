using Godot;
using System.Linq;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;
using AiTest.Godot;

public partial class GameManager : Node2D
{
    private AiTest.Core.Engine.GameEngine? _engine;
    private GameUI? _gameUI;
    private MapRenderer? _mapRenderer;
    private RobotRenderer? _robotRenderer;

    private const int MapWidth = 12;
    private const int MapHeight = 12;

    private int _currentRegister;
    private bool _programmingPhase = true;
    private int _roundNumber = 1;

    public override void _Ready()
    {
        GD.Print("=== RoboRally Game Starting ===");

        InitializeGame();
        SetupLayers();
        CreateTestLevel();

        GD.Print("=== Game Ready! ===");
    }

    private void InitializeGame()
    {
        var availableCards = new System.Collections.Generic.List<ProgramCard>
        {
            new MoveCard { Priority = 100, Spaces = 1 },
            new MoveCard { Priority = 101, Spaces = 2 },
            new MoveCard { Priority = 102, Spaces = 3 },
            new TurnCard { Priority = 150, IsLeft = true },
            new TurnCard { Priority = 151, IsLeft = false },
            new UTurnCard { Priority = 350 },
            new MoveCard { Priority = 103, Spaces = 1 },
            new MoveCard { Priority = 104, Spaces = 2 },
            new MoveCard { Priority = 105, Spaces = 3 }
        };

        var dealer = new FixedDealer(availableCards);
        _engine = new GameEngine(dealer);
        var map = new GameMap(MapWidth, MapHeight);
        _engine.Initialize(map);

        _engine.AddPlayer("Player1");
        _engine.AddPlayer("Player2");

        _engine.StartGame();
    }

    private void SetupLayers()
    {
        _mapRenderer = new MapRenderer { Name = "MapLayer" };
        AddChild(_mapRenderer);

        _robotRenderer = new RobotRenderer { Name = "RobotLayer" };
        AddChild(_robotRenderer);

        _gameUI = new GameUI { Name = "UILayer" };
        AddChild(_gameUI);
    }

    private void CreateTestLevel()
    {
        var map = _engine!.State.Map;

        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                map.SetTile(x, y, Tile.Floor);
            }
        }

        map.SetTile(1, 1, Tile.Start);
        map.SetTile(10, 1, Tile.Start);
        map.SetTile(1, 10, Tile.Start);
        map.SetTile(10, 10, Tile.Start);

        map.SetTile(5, 5, Tile.Checkpoint(1));
        map.SetTile(6, 5, Tile.Checkpoint(2));
        map.SetTile(7, 5, Tile.Checkpoint(3));
        map.SetTile(8, 5, Tile.Checkpoint(4));

        map.SetTile(3, 3, Tile.Wall);
        map.SetTile(8, 3, Tile.Wall);
        map.SetTile(3, 8, Tile.Wall);

        map.SetTile(5, 2, Tile.Pit);

        _mapRenderer!.RenderMap(map);
        _robotRenderer!.CreateRobots(_engine.State.Players);
        UpdateUI();
    }

    public override void _Process(double delta)
    {
        HandleInput();
        _robotRenderer!.UpdatePositions(_engine!.State.Players);
        UpdateUI();
    }

    private void HandleInput()
    {
        if (_engine == null) return;

        if (_programmingPhase)
        {
            if (Input.IsActionJustPressed("ui_accept"))
            {
                _programmingPhase = false;
                _currentRegister = 0;
                return;
            }

            for (int i = 0; i < 9; i++)
            {
                string actionName = $"ui_{i + 1}";
                if (Input.IsActionJustPressed(actionName))
                {
                    var player = _engine.State.Players[0];
                    var card = player.Hand.ElementAtOrDefault(i);
                    if (card != null && _currentRegister < 5)
                    {
                        player.SetRegister(_currentRegister, card);
                        _currentRegister++;

                        if (_currentRegister >= 5)
                        {
                            _programmingPhase = false;
                        }
                    }
                }
            }
        }
        else
        {
            if (Input.IsActionJustPressed("ui_accept"))
            {
                ExecuteCurrentRegister();
            }
        }

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            GetTree().ReloadCurrentScene();
        }
    }

    private void ExecuteCurrentRegister()
    {
        if (_engine == null || _currentRegister >= 5) return;

        var currentCard = _engine.State.Players[0].GetRegister(_currentRegister);
        var priority = currentCard?.Priority ?? 0;
        _gameUI!.PriorityText = $"Priority: {priority}";

        _engine.ExecuteRegister(_currentRegister);
        _currentRegister++;

        if (_currentRegister >= 5)
        {
            _currentRegister = 0;
            _programmingPhase = true;
            _roundNumber++;
        }
    }

    private void UpdateUI()
    {
        if (_engine == null || _gameUI == null) return;

        _gameUI.UpdateRound(_roundNumber);
        _gameUI.UpdatePhase(_programmingPhase);

        if (_programmingPhase)
        {
            _gameUI.UpdateRegisterPrompt(_currentRegister);
        }
        else
        {
            _gameUI.UpdateExecutionPrompt(_currentRegister);
        }

        var p1 = _engine.State.Players[0];
        var p2 = _engine.State.Players[1];
        _gameUI.UpdatePlayerInfo(p1, p2, _currentRegister, _programmingPhase);
    }
}
