using Godot;
using System.Collections.Generic;
using System.Linq;
using AiTest.Core.Engine;
using AiTest.Core.Entities;
using AiTest.Godot;
using AiTest.Godot.Phases;
using AiTest.Core.Messages;

public partial class GameManager : Node2D
{
	private GameRunner? _runner;
	private GameUI? _gameUI;
	private MapRenderer? _mapRenderer;
	private RobotRenderer? _robotRenderer;
	private AnimationController? _animationController;
	private MessageDispatcher _dispatcher = new();

	private IGamePhase? _currentPhase;
	private ProgrammingPhase? _programmingPhase;
	private ExecutionPhase? _executionPhase;
	private GamePhaseType _phaseType = GamePhaseType.Programming;
	private int _roundNumber = 1;

	public override void _Ready()
	{
		GD.Print("=== RoboRally Game Starting ===");

		InitializeGame();
		SetupLayers();
		CreateTestLevel();
		InitializeAnimationController();
		InitializePhases();
		SubscribeToEvents();

		GD.Print("=== Game Ready! ===");
	}

	private void InitializeGame()
	{
		_runner = new GameRunner(GameRunner.CreateDefault2Player());
		_runner.Initialize();
	}

	private void InitializePhases()
	{
		_programmingPhase = new ProgrammingPhase();
		_executionPhase = new ExecutionPhase();

		_programmingPhase.Initialize(_runner!, _dispatcher, OnPhaseComplete);
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
		_animationController?.ClearLog();
		_animationController?.AddLogMessage($"[b]=== Round {e.Round} Execution ===[/b]");
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

		_robotRenderer = new RobotRenderer { Name = "RobotLayer" };
		AddChild(_robotRenderer);

		var effectsLayer = new Node2D { Name = "EffectsLayer" };
		AddChild(effectsLayer);

		_animationController = new AnimationController { Name = "AnimationController" };
		AddChild(_animationController);
		
		_animationController.SetEffectsLayer(effectsLayer);

		_gameUI = GetNodeOrNull<GameUI>("UILayer/GameUI");
		GD.Print("[GameManager] GameUI loaded from scene: " + (_gameUI != null));
	}

	private void InitializeAnimationController()
	{
		if (_runner == null || _robotRenderer == null || _animationController == null) return;
		
		_animationController.Initialize(_runner, _robotRenderer);
		
		var logLabel = GetNodeOrNull<RichTextLabel>("UILayer/GameUI/ActionLogPanel/VBox/ScrollContainer/LogLabel");
		if (logLabel != null)
		{
			_animationController.SetActionLog(logLabel);
		}
		
		GD.Print("[GameManager] AnimationController initialized");
	}

	private void CreateTestLevel()
	{
		var map = _runner!.Engine.State.Map;
		_mapRenderer!.RenderMap(map);
		_robotRenderer!.CreateRobots(_runner.Engine.State.Players);
		UpdateUI();
	}

	public override void _Process(double delta)
	{
		HandleInput();
		_currentPhase?.Update(delta);
		_robotRenderer!.UpdatePositions(_runner!.Engine.State.Players);
		UpdateUI();
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
