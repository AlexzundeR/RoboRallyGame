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
	private Node2D? _mapLayer;
	private Node2D? _gameLayer;
	private readonly Dictionary<string, Label> _uiLabels = new();
	
	private const int TILE_SIZE = 64;
	private const int MAP_WIDTH = 12;
	private const int MAP_HEIGHT = 12;
	
	private int _currentRegister = -1;
	private bool _programmingPhase = true;
	private int _roundNumber = 1;

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
		
		var gameInfoPanel = new PanelContainer();
		gameInfoPanel.Name = "GameInfoPanel";
		gameInfoPanel.SetAnchorsPreset(Control.LayoutPreset.RightWide);
		gameInfoPanel.OffsetLeft = -340;
		gameInfoPanel.OffsetTop = 10;
		gameInfoPanel.OffsetRight = -10;
		gameInfoPanel.OffsetBottom = 360;
		canvasLayer.AddChild(gameInfoPanel);
		
		var vbox = new VBoxContainer();
		vbox.Name = "VBox";
		vbox.AddThemeConstantOverride("separation", 6);
		gameInfoPanel.AddChild(vbox);
		
		var title = new Label();
		title.Text = "🤖 ROBO RALLY";
		title.HorizontalAlignment = HorizontalAlignment.Center;
		title.AddThemeFontSizeOverride("font_size", 18);
		vbox.AddChild(title);
		
		var roundLabel = new Label();
		roundLabel.Name = "RoundLabel";
		roundLabel.Text = "Round: 1";
		vbox.AddChild(roundLabel);
		_uiLabels["round"] = roundLabel;
		
		var phaseLabel = new Label();
		phaseLabel.Name = "PhaseLabel";
		phaseLabel.Text = "Phase: PROGRAMMING";
		phaseLabel.AddThemeColorOverride("font_color", Colors.Yellow);
		vbox.AddChild(phaseLabel);
		_uiLabels["phase"] = phaseLabel;
		
		var registerLabel = new Label();
		registerLabel.Name = "RegisterLabel";
		registerLabel.Text = "Select registers with 1-5";
		vbox.AddChild(registerLabel);
		_uiLabels["register"] = registerLabel;
		
		vbox.AddChild(new HSeparator());
		
		var p1Header = new Label();
		p1Header.Text = "🔵 PLAYER 1";
		p1Header.AddThemeColorOverride("font_color", Colors.DodgerBlue);
		vbox.AddChild(p1Header);
		
		var p1Registers = new Label();
		p1Registers.Name = "Player1Registers";
		p1Registers.Text = "Registers: [ - ][ - ][ - ][ - ][ - ]";
		p1Registers.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		vbox.AddChild(p1Registers);
		_uiLabels["p1_registers"] = p1Registers;
		
		var p1Status = new Label();
		p1Status.Name = "Player1Status";
		p1Status.Text = "Pos: (0,0) North | ❤️ 0";
		vbox.AddChild(p1Status);
		_uiLabels["p1_status"] = p1Status;
		
		vbox.AddChild(new HSeparator());
		
		var p2Header = new Label();
		p2Header.Text = "🔴 PLAYER 2";
		p2Header.AddThemeColorOverride("font_color", Colors.Red);
		vbox.AddChild(p2Header);
		
		var p2Registers = new Label();
		p2Registers.Name = "Player2Registers";
		p2Registers.Text = "Registers: [ - ][ - ][ - ][ - ][ - ]";
		p2Registers.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		vbox.AddChild(p2Registers);
		_uiLabels["p2_registers"] = p2Registers;
		
		var p2Status = new Label();
		p2Status.Name = "Player2Status";
		p2Status.Text = "Pos: (0,0) North | ❤️ 0";
		vbox.AddChild(p2Status);
		_uiLabels["p2_status"] = p2Status;
		
		vbox.AddChild(new HSeparator());
		
		var priorityLabel = new Label();
		priorityLabel.Name = "PriorityLabel";
		priorityLabel.Text = "Priority: -";
		priorityLabel.AddThemeColorOverride("font_color", Colors.Orange);
		vbox.AddChild(priorityLabel);
		_uiLabels["priority"] = priorityLabel;
		
		var controlsPanel = new PanelContainer();
		controlsPanel.Name = "ControlsPanel";
		controlsPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
		controlsPanel.OffsetLeft = 10;
		controlsPanel.OffsetTop = -130;
		controlsPanel.OffsetRight = 450;
		controlsPanel.OffsetBottom = -10;
		canvasLayer.AddChild(controlsPanel);
		
		var controlsVbox = new VBoxContainer();
		controlsVbox.AddThemeConstantOverride("separation", 2);
		controlsPanel.AddChild(controlsVbox);
		
		var title2 = new Label();
		title2.Text = "🎮 CONTROLS";
		title2.HorizontalAlignment = HorizontalAlignment.Center;
		controlsVbox.AddChild(title2);
		
		var controls = new Label();
		controls.Text = 
			"1-5: Set register card (Programming phase)\n" +
			"SPACE: Execute register (Execution phase)\n" +
			"R: Restart game";
		controlsVbox.AddChild(controls);
	}

	private void InitializeGame()
	{
		var dealer = new FixedDealer(
			new List<ProgramCard>
			{
				new MoveCard { Priority = 100, Spaces = 1 },
				new MoveCard { Priority = 101, Spaces = 2 },
				new MoveCard { Priority = 102, Spaces = 3 },
				new TurnCard { Priority = 150, IsLeft = true },
				new TurnCard { Priority = 151, IsLeft = false },
				new UTurnCard { Priority = 350 },
				new MoveCard { Priority = 103, Spaces = 1 },
				new MoveCard { Priority = 104, Spaces = 2 },
				new MoveCard { Priority = 105, Spaces = 3 },
			}
		);
		
		_engine = new GameEngine(dealer);
		var map = new GameMap(MAP_WIDTH, MAP_HEIGHT);
		_engine.Initialize(map);
		
		_engine.AddPlayer("Player1");
		_engine.AddPlayer("Player2");
		
		_engine.StartGame();
		
		var p1 = _engine.State.Players[0];
		var p2 = _engine.State.Players[1];
		
		p1.SetRegister(0, new MoveCard { Priority = 100, Spaces = 2 });
		p1.SetRegister(1, new TurnCard { Priority = 150, IsLeft = true });
		p1.SetRegister(2, new MoveCard { Priority = 101, Spaces = 1 });
		p1.SetRegister(3, new UTurnCard { Priority = 350 });
		p1.SetRegister(4, new MoveCard { Priority = 102, Spaces = 3 });
		
		p2.SetRegister(0, new TurnCard { Priority = 151, IsLeft = false });
		p2.SetRegister(1, new MoveCard { Priority = 103, Spaces = 1 });
		p2.SetRegister(2, new MoveCard { Priority = 104, Spaces = 2 });
		p2.SetRegister(3, new TurnCard { Priority = 150, IsLeft = true });
		p2.SetRegister(4, new MoveCard { Priority = 105, Spaces = 3 });
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
					player.Robot.Position.X * TILE_SIZE + 12,
					player.Robot.Position.Y * TILE_SIZE + 12
				),
				Size = new Vector2(TILE_SIZE - 24, TILE_SIZE - 24),
				Color = player.Id == 1 ? Colors.DodgerBlue : Colors.Red
			};
			
			var directionIndicator = new ColorRect
			{
				Position = new Vector2(TILE_SIZE / 2 - 4, 4),
				Size = new Vector2(8, 12),
				Color = Colors.White
			};
			robot.AddChild(directionIndicator);
			
			var healthBar = new ColorRect
			{
				Position = new Vector2(4, TILE_SIZE - 32),
				Size = new Vector2(TILE_SIZE - 32, 4),
				Color = Colors.Green
			};
			robot.AddChild(healthBar);
			
			_gameLayer!.AddChild(robot);
			_robotSprites[player.Id] = robot;
		}
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
			_currentRegister = -1;
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
					player.Robot.Position.X * TILE_SIZE + 12,
					player.Robot.Position.Y * TILE_SIZE + 12
				);
				
				sprite.Position = sprite.Position.Lerp(targetPos, 0.15f);
				
				if (player.Robot.IsAlive == false)
				{
					sprite.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
				}
			}
		}
	}

	private void UpdateUI()
	{
		if (_engine == null) return;
		
		_uiLabels["round"].Text = $"Round: {_roundNumber}";
		
		if (_programmingPhase)
		{
			_uiLabels["phase"].Text = "Phase: PROGRAMMING";
			_uiLabels["phase"].AddThemeColorOverride("font_color", Colors.Yellow);
			_uiLabels["register"].Text = "Press SPACE to start execution";
			_uiLabels["priority"].Text = "Priority: -";
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
		var p1Alive = p1.Robot.IsAlive ? "❤️" : "💀";
		var p1Dir = GetDirectionSymbol(p1.Robot.Direction);
		_uiLabels["p1_status"].Text = $"Pos: ({p1.Robot.Position.X},{p1.Robot.Position.Y}) {p1Dir} | {p1.Robot.Damage} HP {p1Alive}";
		
		_uiLabels["p2_registers"].Text = $"Registers: {FormatRegisters(p2)}";
		var p2Alive = p2.Robot.IsAlive ? "❤️" : "💀";
		var p2Dir = GetDirectionSymbol(p2.Robot.Direction);
		_uiLabels["p2_status"].Text = $"Pos: ({p2.Robot.Position.X},{p2.Robot.Position.Y}) {p2Dir} | {p2.Robot.Damage} HP {p2Alive}";
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
