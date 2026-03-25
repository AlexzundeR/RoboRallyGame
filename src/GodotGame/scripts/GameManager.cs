using Godot;
using System.Collections.Generic;
using System.Linq;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Godot;
using AiTest.Godot.Animations;
using AiTest.Godot.Phases;
using AiTest.Core.Messages;

public partial class GameManager : Node2D
{
	private GameRunner? _runner;
	private GameUI? _gameUI;
	private MapRenderer? _mapRenderer;
	private RobotRenderer? _robotRenderer;
	private AnimationService? _animationService;
	private CameraController? _camera;
	private MessageDispatcher _dispatcher = new();

	private IGamePhase? _currentPhase;
	private ProgrammingPhase? _programmingPhase;
	private ExecutionPhase? _executionPhase;
	private GamePhaseType _phaseType = GamePhaseType.Programming;
	private int _roundNumber = 1;

	private bool _isDragging;
	private Vector2 _dragStartMousePos;
	private Vector2 _dragStartCameraPos;

	public override void _Ready()
	{
		GD.Print("=== RoboRally Game Starting ===");

		InitializeGame();
		SetupLayers();
		CreateTestLevel();
		InitializePhases();
		InitializeAnimationController();
		SubscribeToEvents();

		GD.Print("=== Game Ready! ===");
	}

	public override void _Input(InputEvent @event)
	{
		HandleCameraInput(@event);
	}

	private void HandleCameraInput(InputEvent @event)
	{
		if (_camera == null)
			return;

		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left || mouseButton.ButtonIndex == MouseButton.Middle)
			{
				if (mouseButton.Pressed)
				{
					_isDragging = true;
					_dragStartMousePos = GetViewport().GetMousePosition();
					_dragStartCameraPos = _camera.Position;
				}
				else
				{
					_isDragging = false;
				}
			}
		}

		if (@event is InputEventMouseMotion motion && _isDragging)
		{
			var currentMousePos = GetViewport().GetMousePosition();
			var delta = currentMousePos - _dragStartMousePos;
			_camera.Position = _dragStartCameraPos - delta / _camera.Zoom;
		}
	}

	private void InitializeGame()
	{
		_runner = new GameRunner(GameMapConfigs.CreateDefault2Player());
		_runner.Initialize();
	}

	private void InitializePhases()
	{
		_programmingPhase = new ProgrammingPhase();
		_executionPhase = new ExecutionPhase();

		_programmingPhase.Initialize(_runner!, _dispatcher, OnPhaseComplete);
		_executionPhase.SetAnimationService(_animationService!);
		_executionPhase.SetRobotLayer(_robotRenderer!);
		_executionPhase.Initialize(_runner!, _dispatcher, OnPhaseComplete);

		SetPhase(GamePhaseType.Programming);
	}

	private void SubscribeToEvents()
	{
		_dispatcher.Subscribe<CardSelectedEvent>(OnCardSelected);
		_dispatcher.Subscribe<PlayerConfirmedEvent>(OnPlayerConfirmed);
		_dispatcher.Subscribe<RegisterExecutedEvent>(OnRegisterExecuted);
		_dispatcher.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
		_dispatcher.Subscribe<RegisterSelectionChangedEvent>(OnRegisterSelectionChanged);
		_dispatcher.Subscribe<ExecutionStartedEvent>(OnExecutionStarted);
		_dispatcher.Subscribe<RegistersClearedEvent>(OnRegistersCleared);
	}

	private void OnExecutionStarted(ExecutionStartedEvent e)
	{
	}

	private void OnRegistersCleared(RegistersClearedEvent e)
	{
		UpdateUI();
	}

	private void OnRegisterSelectionChanged(RegisterSelectionChangedEvent e)
	{
		UpdateUI();
	}

	private void OnCardSelected(CardSelectedEvent e)
	{
		UpdateUI();
	}

	private void OnPlayerConfirmed(PlayerConfirmedEvent e)
	{
		UpdateUI();
	}

	private void OnRegisterExecuted(RegisterExecutedEvent e)
	{
		_robotRenderer!.UpdatePositions(_runner!.Engine.State.Players);
		UpdateUI();
	}

	private void OnPhaseChanged(PhaseChangedEvent e)
	{
		_phaseType = e.Phase;
		_roundNumber = e.Round;
		UpdateUI();
	}

	private void OnPhaseComplete(GamePhaseType newPhase)
	{
		if (newPhase == GamePhaseType.Programming)
		{
			_roundNumber++;
		}
		_dispatcher.Publish(new PhaseChangedEvent(newPhase, _roundNumber));
		SetPhase(newPhase);
		UpdateUI();
	}

	private void SetPhase(GamePhaseType phase)
	{
		_currentPhase?.OnExit();
		_phaseType = phase;

		_currentPhase = phase switch
		{
			GamePhaseType.Programming => _programmingPhase,
			GamePhaseType.Execution => _executionPhase,
			_ => null
		};

		if (phase == GamePhaseType.Execution)
		{
			_dispatcher.Publish(new ExecutionStartedEvent(_roundNumber));
		}

		_currentPhase?.OnEnter();
	}

	private void SetupLayers()
	{
		_mapRenderer = new MapRenderer { Name = "MapLayer" };
		AddChild(_mapRenderer);

		_camera = new CameraController { Name = "Camera" };
		_mapRenderer.AddChild(_camera);

		_robotRenderer = new RobotRenderer { Name = "RobotLayer" };
		AddChild(_robotRenderer);

		var effectsLayer = new Node2D { Name = "EffectsLayer" };
		AddChild(effectsLayer);

		_animationService = new AnimationService { Name = "AnimationService" };
		AddChild(_animationService);

		_gameUI = GetNodeOrNull<GameUI>("UILayer/GameUI");
		GD.Print("[GameManager] GameUI loaded from scene: " + (_gameUI != null));
	}

	private void InitializeAnimationController()
	{
		if (_animationService == null || _robotRenderer == null || _executionPhase == null)
		{
			GD.PrintErr("[GameManager] InitializeAnimationController: missing dependencies");
			return;
		}
		
		GD.Print("[GameManager] AnimationService initialized");
		
		var logLabel = GetNodeOrNull<RichTextLabel>("UILayer/GameUI/ActionLogPanel/VBox/ScrollContainer/LogLabel");
		if (logLabel != null)
		{
			_executionPhase.SetActionLog(logLabel);
			GD.Print("[GameManager] ActionLog connected to ExecutionPhase");
		}
		else
		{
			GD.PrintErr("[GameManager] ActionLog not found in scene!");
		}
		
		GD.Print("[GameManager] Animation setup complete");
	}

	private void CreateTestLevel()
	{
		var map = _runner!.Engine.State.Map;
		_mapRenderer!.RenderMap(map);
		_robotRenderer!.CreateRobots(_runner.Engine.State.Players);
		UpdateUI();
		CenterCamera(map.Width, map.Height);
	}

	private void CenterCamera(int mapWidth, int mapHeight)
	{
		if (_camera == null)
			return;

		const int tileSize = 48;
		const int mapOffsetY = 120;
		const int actionLogWidth = 280;
		var screenWidth = GetViewport().GetVisibleRect().Size.X;
		var availableWidth = screenWidth - actionLogWidth;
		var centerX = availableWidth / 2f;
		var centerY = mapHeight * tileSize / 2f + mapOffsetY;
		_camera.Position = new Vector2(centerX, centerY);
	}

	public override void _Process(double delta)
	{
		HandleCameraKeyboardPan(delta);
		HandleInput();
		_currentPhase?.Update(delta);
		_robotRenderer!.UpdatePositions(_runner!.Engine.State.Players);
		UpdateUI();
	}

	private void HandleCameraKeyboardPan(double delta)
	{
		if (_camera == null)
			return;

		const float panSpeed = 400f;
		Vector2 moveDir = Vector2.Zero;

		if (Input.IsKeyPressed(Key.W))
			moveDir.Y -= 1;
		if (Input.IsKeyPressed(Key.S))
			moveDir.Y += 1;
		if (Input.IsKeyPressed(Key.A))
			moveDir.X -= 1;
		if (Input.IsKeyPressed(Key.D))
			moveDir.X += 1;

		if (moveDir != Vector2.Zero)
		{
			_camera.Position -= moveDir.Normalized() * panSpeed * (float)delta;
		}
	}

	private void HandleInput()
	{
		if (_currentPhase == null)
		{
			GD.Print("[GameManager] HandleInput: _currentPhase is null, returning");
			return;
		}
		
		_currentPhase.HandleInput();

		if (Input.IsActionJustPressed("ui_cancel"))
		{
			GetTree().ReloadCurrentScene();
		}
	}

	private void UpdateUI()
	{
		if (_runner == null || _gameUI == null) 
		{
			GD.Print($"[GameManager] UpdateUI: runner={_runner != null}, gameUI={_gameUI != null}, returning");
			return;
		}

		var p1 = _runner.Engine.State.Players[0];
		var p2 = _runner.Engine.State.Players[1];

		_gameUI.UpdateRound(_roundNumber);
		_gameUI.UpdatePhase(_phaseType == GamePhaseType.Programming);

		if (_phaseType == GamePhaseType.Programming && _programmingPhase != null)
		{
			_gameUI.UpdateRegisterPrompt(
				_programmingPhase.CurrentRegister,
				_programmingPhase.SelectedRegister,
				_programmingPhase.CurrentPlayerName
			);
		}
		else if (_phaseType == GamePhaseType.Execution && _executionPhase != null)
		{
			_gameUI.UpdateExecutionPrompt(
				_executionPhase.CurrentRegister,
				_executionPhase.CurrentPlayerName
			);
			_gameUI.PriorityText = $"Priority: {_executionPhase.CurrentPriority}";
		}
		
		int currentPlayerIdx = _phaseType == GamePhaseType.Programming
			? _programmingPhase?.CurrentPlayerIndex ?? 0
			: _executionPhase?.CurrentPlayerIndex ?? 0;
			
		int selectedReg = _phaseType == GamePhaseType.Programming
			? (_programmingPhase?.SelectedRegister ?? 0)
			: 0;
			
		_gameUI.UpdatePlayerInfo(p1, p2, 
			_phaseType == GamePhaseType.Programming 
				? (_programmingPhase?.CurrentRegister ?? 0)
				: (_executionPhase?.CurrentRegister ?? 0),
			_phaseType == GamePhaseType.Programming,
			currentPlayerIdx,
			selectedReg
		);
	}
}
