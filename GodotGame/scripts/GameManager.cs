using Godot;
using System.Collections.Generic;
using System.Linq;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Core.Models;

public partial class GameManager : Node2D
{
	private AiTest.Core.Engine.GameEngine? _engine;
	private readonly Dictionary<int, ColorRect> _robotSprites = new();
	private readonly Dictionary<int, ColorRect> _robotDirectionIndicators = new();
	private Node2D? _mapLayer;
	private Node2D? _gameLayer;
	private readonly Dictionary<string, Label> _uiLabels = new();
	private readonly List<TextureRect> _registerSlots = new();
	
	private const int TILE_SIZE = 64;
	private const int MAP_WIDTH = 12;
	private const int MAP_HEIGHT = 12;
	
    private int _currentRegister = 0;
	private bool _programmingPhase = true;
	private int _roundNumber = 1;
	private readonly List<ProgramCard> _availableCards = new();

	public override void _Ready()
	{
		GD.Print("=== RoboRally Game Starting ===");
		
		_mapLayer = new Node2D();
		_mapLayer.Name = "MapLayer";
		AddChild(_mapLayer);
		
		_gameLayer = new Node2D();
		_gameLayer.Name = "GameLayer";
		AddChild(_gameLayer);
		
		CreateUI();
		InitializeGame();
		CreateTestLevel();
		UpdateUI();
		
		GD.Print("=== Game Ready! ===");
	}

	private void CreateUI()
	{
		var canvasLayer = new CanvasLayer();
		canvasLayer.Name = "UILayer";
		AddChild(canvasLayer);
		
		var topPanel = new PanelContainer();
		topPanel.Name = "TopPanel";
		topPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
		topPanel.OffsetLeft = 10;
		topPanel.OffsetTop = 10;
		topPanel.OffsetRight = -10;
		topPanel.OffsetBottom = 80;
		canvasLayer.AddChild(topPanel);
		
		var topHBox = new HBoxContainer();
		topHBox.AddThemeConstantOverride("separation", 20);
		topPanel.AddChild(topHBox);
		
		var leftVBox = new VBoxContainer();
		leftVBox.AddThemeConstantOverride("separation", 4);
		topHBox.AddChild(leftVBox);
		
		var title = new Label();
		title.Text = "🤖 ROBO RALLY";
		title.AddThemeFontSizeOverride("font_size", 24);
		leftVBox.AddChild(title);
		
		var roundLabel = new Label();
		roundLabel.Name = "RoundLabel";
		roundLabel.Text = "Round: 1";
		leftVBox.AddChild(roundLabel);
		_uiLabels["round"] = roundLabel;
		
		var phaseLabel = new Label();
		phaseLabel.Name = "PhaseLabel";
		phaseLabel.Text = "Phase: PROGRAMMING";
		phaseLabel.AddThemeColorOverride("font_color", Colors.Yellow);
		phaseLabel.AddThemeFontSizeOverride("font_size", 18);
		leftVBox.AddChild(phaseLabel);
		_uiLabels["phase"] = phaseLabel;
		
		var middleVBox = new VBoxContainer();
		middleVBox.AddThemeConstantOverride("separation", 4);
		topHBox.AddChild(middleVBox);
		
		var cardSelectionLabel = new Label();
		cardSelectionLabel.Text = "🎴 SELECT CARDS FOR REGISTERS";
		cardSelectionLabel.AddThemeFontSizeOverride("font_size", 16);
		middleVBox.AddChild(cardSelectionLabel);
		
		var cardContainer = new HBoxContainer();
		cardContainer.AddThemeConstantOverride("separation", 8);
		middleVBox.AddChild(cardContainer);
		
		for (int i = 0; i < 9; i++)
		{
			var cardSlot = new TextureRect();
			cardSlot.CustomMinimumSize = new Vector2(50, 60);
			cardSlot.Modulate = new Color(0.7f, 0.7f, 0.7f);
			cardContainer.AddChild(cardSlot);
		}
		
		var rightVBox = new VBoxContainer();
		rightVBox.AddThemeConstantOverride("separation", 4);
		topHBox.AddChild(rightVBox);
		
		var priorityLabel = new Label();
		priorityLabel.Name = "PriorityLabel";
		priorityLabel.Text = "Priority: -";
		priorityLabel.AddThemeColorOverride("font_color", Colors.Orange);
		priorityLabel.AddThemeFontSizeOverride("font_size", 16);
		rightVBox.AddChild(priorityLabel);
		_uiLabels["priority"] = priorityLabel;
		
		var helpLabel = new Label();
		helpLabel.Text = "1-5: Set register | SPACE: Execute";
		helpLabel.AddThemeColorOverride("font_color", Colors.Gray);
		rightVBox.AddChild(helpLabel);
		
		var registerLabel = new Label();
		registerLabel.Name = "RegisterLabel";
		registerLabel.Text = "Select card for register 1/5 (keys 1-9)";
		registerLabel.AddThemeColorOverride("font_color", Colors.Cyan);
		registerLabel.AddThemeFontSizeOverride("font_size", 14);
		rightVBox.AddChild(registerLabel);
		_uiLabels["register"] = registerLabel;
		
		var bottomPanel = new PanelContainer();
		bottomPanel.Name = "BottomPanel";
		bottomPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
		bottomPanel.OffsetLeft = 10;
		bottomPanel.OffsetTop = -90;
		bottomPanel.OffsetRight = -10;
		bottomPanel.OffsetBottom = -10;
		canvasLayer.AddChild(bottomPanel);
		
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
		
		var header = new Label();
		header.Text = title;
		header.AddThemeColorOverride("font_color", color);
		header.AddThemeFontSizeOverride("font_size", 16);
		vbox.AddChild(header);
		
		var registersLabel = new Label();
		registersLabel.Name = $"{prefix}_registers";
		registersLabel.Text = "Registers: [ - ][ - ][ - ][ - ][ - ]";
		vbox.AddChild(registersLabel);
		_uiLabels[$"{prefix}_registers"] = registersLabel;
		
		var statusLabel = new Label();
		statusLabel.Name = $"{prefix}_status";
		statusLabel.Text = "Pos: (0,0) ⬆ | ❤️ 0";
		vbox.AddChild(statusLabel);
		_uiLabels[$"{prefix}_status"] = statusLabel;
		
		return vbox;
	}

	private void InitializeGame()
	{
		_availableCards.Add(new MoveCard { Priority = 100, Spaces = 1 });
		_availableCards.Add(new MoveCard { Priority = 101, Spaces = 2 });
		_availableCards.Add(new MoveCard { Priority = 102, Spaces = 3 });
		_availableCards.Add(new TurnCard { Priority = 150, IsLeft = true });
		_availableCards.Add(new TurnCard { Priority = 151, IsLeft = false });
		_availableCards.Add(new UTurnCard { Priority = 350 });
		_availableCards.Add(new MoveCard { Priority = 103, Spaces = 1 });
		_availableCards.Add(new MoveCard { Priority = 104, Spaces = 2 });
		_availableCards.Add(new MoveCard { Priority = 105, Spaces = 3 });
		
		var dealer = new FixedDealer(_availableCards);
		
		_engine = new GameEngine(dealer);
		var map = new GameMap(MAP_WIDTH, MAP_HEIGHT);
		_engine.Initialize(map);
		
		_engine.AddPlayer("Player1");
		_engine.AddPlayer("Player2");
		
		_engine.StartGame();
	}

	private void CreateTestLevel()
	{
		var map = _engine!.State.Map;
		
		for (int x = 0; x < MAP_WIDTH; x++)
		{
			for (int y = 0; y < MAP_HEIGHT; y++)
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
		
		RenderMap();
		CreateRobotSprites();
	}

	private void RenderMap()
	{
		var map = _engine!.State.Map;
		
		for (int x = 0; x < map.Width; x++)
		{
			for (int y = 0; y < map.Height; y++)
			{
				var tile = map.GetTile(new Position(x, y));
				var color = GetTileColor(tile);
				CreateTile(x, y, color);
				
				if (tile.Type == TileType.Checkpoint)
				{
					CreateCheckpointMarker(x, y, tile.CheckpointNumber ?? 1);
				}
			}
		}
	}

	private Color GetTileColor(Tile tile)
	{
		return tile.Type switch
		{
			TileType.Floor => new Color(0.2f, 0.2f, 0.25f),
			TileType.Wall => new Color(0.4f, 0.4f, 0.45f),
			TileType.Start => new Color(0.15f, 0.35f, 0.15f),
			TileType.Pit => new Color(0.05f, 0.05f, 0.1f),
			TileType.Checkpoint => new Color(0.3f, 0.25f, 0.1f),
			TileType.ConveyorCW or TileType.ConveyorCCW => new Color(0.25f, 0.4f, 0.5f),
			TileType.ExpressConveyorCW or TileType.ExpressConveyorCCW => new Color(0.15f, 0.5f, 0.7f),
			TileType.GearCW or TileType.GearCCW => new Color(0.5f, 0.3f, 0.3f),
			_ => new Color(0.2f, 0.2f, 0.25f)
		};
	}

	private void CreateTile(int x, int y, Color color)
	{
		var tile = new ColorRect
		{
			Position = new Vector2(x * TILE_SIZE, y * TILE_SIZE),
			Size = new Vector2(TILE_SIZE, TILE_SIZE),
			Color = color
		};
		
		var border = new ColorRect
		{
			Position = new Vector2(x * TILE_SIZE + 1, y * TILE_SIZE + 1),
			Size = new Vector2(TILE_SIZE - 2, TILE_SIZE - 2),
			Color = new Color(color.R + 0.1f, color.G + 0.1f, color.B + 0.1f)
		};
		
		_mapLayer!.AddChild(tile);
		_mapLayer!.AddChild(border);
	}

	private void CreateCheckpointMarker(int x, int y, int number)
	{
		var label = new Label();
		label.Text = "★" + number;
		label.Position = new Vector2(x * TILE_SIZE + 16, y * TILE_SIZE + 20);
		label.AddThemeFontSizeOverride("font_size", 24);
		label.AddThemeColorOverride("font_color", Colors.Gold);
		_mapLayer!.AddChild(label);
	}

	private void CreateRobotSprites()
	{
		foreach (var player in _engine!.State.Players)
		{
			var robot = new ColorRect
			{
				Name = $"Robot_{player.Id}",
				Position = new Vector2(
					player.Robot.Position.X * TILE_SIZE + 10,
					player.Robot.Position.Y * TILE_SIZE + 10
				),
				Size = new Vector2(TILE_SIZE - 20, TILE_SIZE - 20),
				Color = player.Id == 1 ? Colors.DodgerBlue : Colors.Red
			};
			
			var directionIndicator = new ColorRect
			{
				Name = "DirectionIndicator",
				Size = new Vector2(12, 12),
				Color = Colors.Yellow
			};
			robot.AddChild(directionIndicator);
			_robotDirectionIndicators[player.Id] = directionIndicator;
			
			var healthBar = new ColorRect
			{
				Position = new Vector2(4, TILE_SIZE - 28),
				Size = new Vector2(TILE_SIZE - 28, 4),
				Color = Colors.Green
			};
			robot.AddChild(healthBar);
			
			_gameLayer!.AddChild(robot);
			_robotSprites[player.Id] = robot;
		}
		
		UpdateRobotDirections();
	}

	public override void _Process(double delta)
	{
		HandleInput();
		UpdateRobotPositions();
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
				UpdateUI();
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
						UpdateUI();
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
		_uiLabels["priority"].Text = $"Priority: {priority}";
		
		_engine.ExecuteRegister(_currentRegister);
		_currentRegister++;
		
		if (_currentRegister >= 5)
		{
			_currentRegister = 0;
			_programmingPhase = true;
			_roundNumber++;
		}
		
		UpdateUI();
	}

	private void UpdateRobotPositions()
	{
		if (_engine == null) return;
		
		foreach (var player in _engine.State.Players)
		{
			if (_robotSprites.TryGetValue(player.Id, out var sprite))
			{
				var targetPos = new Vector2(
					player.Robot.Position.X * TILE_SIZE + 10,
					player.Robot.Position.Y * TILE_SIZE + 10
				);
				
				sprite.Position = sprite.Position.Lerp(targetPos, 0.15f);
				
				if (player.Robot.IsAlive == false)
				{
					sprite.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
				}
			}
		}
		
		UpdateRobotDirections();
	}

	private void UpdateRobotDirections()
	{
		if (_engine == null) return;
		
		foreach (var player in _engine.State.Players)
		{
			if (_robotDirectionIndicators.TryGetValue(player.Id, out var indicator))
			{
				var (offsetX, offsetY) = GetDirectionOffset(player.Robot.Direction);
				indicator.Position = new Vector2(
					TILE_SIZE / 2 - 6 + offsetX * 15,
					TILE_SIZE / 2 - 6 + offsetY * 15
				);
			}
		}
	}

	private (float, float) GetDirectionOffset(Direction direction)
	{
		return direction switch
		{
			Direction.North => (0, -1),
			Direction.East => (1, 0),
			Direction.South => (0, 1),
			Direction.West => (-1, 0),
			_ => (0, -1)
		};
	}

	private void UpdateUI()
	{
		if (_engine == null) return;
		
		_uiLabels["round"].Text = $"Round: {_roundNumber}";
		
		if (_programmingPhase)
		{
			_uiLabels["phase"].Text = "Phase: PROGRAMMING";
			_uiLabels["phase"].AddThemeColorOverride("font_color", Colors.Yellow);
			
			var player = _engine.State.Players[0];
			var nextReg = _currentRegister < 5 ? _currentRegister : 0;
			_uiLabels["register"].Text = $"Select card for register {nextReg + 1}/5 (keys 1-9)";
		}
		else
		{
			_uiLabels["phase"].Text = $"Phase: EXECUTION";
			_uiLabels["phase"].AddThemeColorOverride("font_color", Colors.Green);
			_uiLabels["register"].Text = $"Executing register {_currentRegister + 1}/5";
		}
		
		var p1 = _engine.State.Players[0];
		var p2 = _engine.State.Players[1];
		
		_uiLabels["p1_registers"].Text = $"Registers: {FormatRegisters(p1)}";
		var p1Dir = GetDirectionSymbol(p1.Robot.Direction);
		var p1Status = p1.Robot.IsAlive ? "❤️" : "💀";
		_uiLabels["p1_status"].Text = $"Pos: ({p1.Robot.Position.X},{p1.Robot.Position.Y}) {p1Dir} | {p1.Robot.Damage} {p1Status}";
		
		_uiLabels["p2_registers"].Text = $"Registers: {FormatRegisters(p2)}";
		var p2Dir = GetDirectionSymbol(p2.Robot.Direction);
		var p2Status = p2.Robot.IsAlive ? "❤️" : "💀";
		_uiLabels["p2_status"].Text = $"Pos: ({p2.Robot.Position.X},{p2.Robot.Position.Y}) {p2Dir} | {p2.Robot.Damage} {p2Status}";
	}

	private string FormatRegisters(Player player)
	{
		var registers = new List<string>();
		for (int i = 0; i < 5; i++)
		{
			var card = player.GetRegister(i);
			var prefix = "";
			var suffix = "";
			
			if (!_programmingPhase && i == _currentRegister)
			{
				prefix = "▶";
				suffix = "◀";
			}
			else if (_programmingPhase && i == _currentRegister)
			{
				prefix = "▼";
			}
			
			if (card == null)
			{
				registers.Add($"{prefix}---{suffix}");
			}
			else
			{
				registers.Add($"{prefix}{GetCardSymbol(card)}{suffix}");
			}
		}
		return $"[{string.Join("][", registers)}]";
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
