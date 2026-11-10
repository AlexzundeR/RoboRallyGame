using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

using RoboRallyGame.Core.Engine;
using RoboRallyGame.Godot;
using RoboRallyGame.Godot.Animations;
using RoboRallyGame.Godot.Phases;
using RoboRallyGame.Godot.Services;
using RoboRallyGame.Core.Messages;
using RoboRallyGame.Core.Models;

public partial class GameManager : Node2D
{
	private GameRunner? _runner;
	private IGameProvider? _gameProvider;
	private GameUI? _gameUI;
	private MapRenderer? _mapRenderer;
	private RobotRenderer? _robotRenderer;
	private AnimationService? _animationService;
	private GameScale? _gameScale;
	private CameraController? _camera;
	private IMessageDispatcher _dispatcher = new MessageDispatcher();

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
		InitializePhases();
		InitializeAnimationController();
		SubscribeToEvents();

		GD.Print("=== Game Ready! ===");
	}

	private void CreateTestLevel()
	{
		var map = _gameProvider!.GetMapData();
		CenterCamera(map.Width, map.Height);
		_mapRenderer!.RenderMap(map);
		_robotRenderer!.CreateRobots(_gameProvider!.GetPlayers());
		UpdateUI();
	}

	public override void _Input(InputEvent @event)
	{
		HandleSecretKeyInput(@event);
	}

	private void HandleSecretKeyInput(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.U)
			{
				GiveRandomUpgradeToCurrentPlayer();
			}
			else if (keyEvent.Keycode == Key.H)
			{
				GiveDamageToCurrentPlayer();
			}
			else if (keyEvent.Keycode == Key.O)
			{
				OpenDebug();
			}
		}

	}

	private void GiveRandomUpgradeToCurrentPlayer()
	{
		if (_runner == null)
			return;

		var player = _runner.Engine.GetCurrentPlayer();
		if (player == null)
			return;

		_runner.Engine.GrantRandomUpgrade(player.Id);
	}

	private void OpenDebug()
	{
		_gameUI.OpenDebugMenu();
	}

	private void GiveDamageToCurrentPlayer()
	{
		if (_runner == null)
			return;

		var player = _runner.Engine.GetCurrentPlayer();
		if (player == null)
			return;

		_runner.Engine.GrantDamage(player.Id, 1);
		GD.Print($"[SECRET] Player {player.Id} took 1 damage");
	}

	private void InitializeGame()
	{
		_runner = new GameRunner(GameMapConfigs.CreateDefault2Player2());
		_runner.Initialize();
		_gameProvider = new LocalGameProvider(_runner.Engine);
		_gameProvider.SubscribeToEngineEvents(_dispatcher);
	}

	public void Initialize(GameRunner runner, IMessageDispatcher dispatcher)
	{
		_runner = runner;
		_gameProvider = new LocalGameProvider(_runner.Engine);
		_dispatcher = dispatcher;
		_gameProvider.SubscribeToEngineEvents(_dispatcher);
	}

	private void InitializePhases()
	{
		_programmingPhase = new ProgrammingPhase();
		_executionPhase = new ExecutionPhase();

		_programmingPhase.Initialize(_gameProvider!, _dispatcher, OnPhaseComplete);
		_executionPhase.SetAnimationService(_animationService!);
		_executionPhase.SetRobotLayer(_robotRenderer!);
		_executionPhase.Initialize(_gameProvider!, _dispatcher, OnPhaseComplete);

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
		_dispatcher.Subscribe<CheckpointEvent>(OnCheckpoint);
		_dispatcher.Subscribe<GameOverEvent>(OnGameOver);
	}

	private void OnCheckpoint(CheckpointEvent e)
	{
	}

	private void OnGameOver(GameOverEvent e)
	{
		var winnerNames = e.WinnerIds.Select(id => $"Player {id}").ToList();
		_gameUI?.ShowGameOver(winnerNames, e.IsTie);
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
		var mapLayer = GetNodeOrNull<Node2D>("MapLayer");
		_gameScale = new GameScale { Name = "GameScale" };
		mapLayer.AddChild(_gameScale);

		var mapContainer = new Node2D { Name = "MapContainer" };
		mapLayer.AddChild(mapContainer);

		_mapRenderer = new MapRenderer { Name = "MapLayer" };
		mapContainer.AddChild(_mapRenderer);

		_robotRenderer = new RobotRenderer { Name = "RobotLayer" };
		mapContainer.AddChild(_robotRenderer);

		_camera = new CameraController { Name = "Camera" };
		mapContainer.AddChild(_camera);

		var effectsLayer = new Node2D { Name = "EffectsLayer" };
		mapLayer.AddChild(effectsLayer);

		_animationService = new AnimationService { Name = "AnimationService" };
		mapLayer.AddChild(_animationService);

		_gameUI = GetNodeOrNull<GameUI>("UILayer/GameUI");
		GD.Print("[GameManager] GameUI loaded from scene: " + (_gameUI != null));

		if (_gameUI != null)
		{
			_gameUI.GiveUpgrade += OnGiveUpgrade;
			_gameUI.GiveDamage += OnGiveDamage;
			GD.Print("[GameManager] Debug signals connected");
		}
	}

	private void OnGiveUpgrade()
	{
		GiveRandomUpgradeToCurrentPlayer();
	}

	private void OnGiveDamage()
	{
		GiveDamageToCurrentPlayer();
	}

	private void InitializeAnimationController()
	{
		if (_animationService == null || _robotRenderer == null || _executionPhase == null)
		{
			GD.PrintErr("[GameManager] InitializeAnimationController: missing dependencies");
			return;
		}

		GD.Print("[GameManager] AnimationService initialized");

		if (_gameUI != null)
		{
			_executionPhase.SetLogAction(_gameUI.Log);
			GD.Print("[GameManager] ActionLog connected to ExecutionPhase");
		}

		GD.Print("[GameManager] Animation setup complete");
	}

	private void CenterCamera(int mapWidth, int mapHeight)
	{
		if (_camera == null)
			return;

		_camera.CenterOn(new Position(mapWidth / 2, mapHeight / 2).ToTilePosition());
	}

	private void OnGameScaleChanged()
	{
		var map = _gameProvider!.GetMapData();
		_gameScale!.Recalculate();
		CenterCamera(map.Width, map.Height);
	}

	private double _UIUpdateTimeCount;
	public override void _Process(double delta)
	{
		//TODO: слишком много моделей перехвата ввода, нужно всё стандартизировать
		HandleInput();
		_currentPhase?.Update(delta);
		_robotRenderer!.UpdatePositions(_gameProvider!.GetPlayers());

		_UIUpdateTimeCount += delta;
		if (_UIUpdateTimeCount > 0.5)
		{
			_UIUpdateTimeCount = 0;
			//TODO: так часто обновлять нельзя, нужно обновлять по сигналам или под флагом
			UpdateUI();
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
			//GetTree().ReloadCurrentScene();
		}
	}

	private object _lock = new Object();
	private void UpdateUI()
	{
		if (_runner == null || _gameUI == null)
		{
			GD.Print($"[GameManager] UpdateUI: runner={_runner != null}, gameUI={_gameUI != null}, returning");
			return;
		}

		lock (_lock)
		{

			var players = _gameProvider!.GetPlayers();

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

			var totalCheckpoints = _gameProvider.GetMapData().TotalCheckpoints;

			_gameUI.UpdatePlayerInfo(players,
				_phaseType == GamePhaseType.Programming
					? (_programmingPhase?.CurrentRegister ?? 0)
					: (_executionPhase?.CurrentRegister ?? 0),
				_phaseType == GamePhaseType.Programming,
				currentPlayerIdx,
				totalCheckpoints,
				selectedReg
			);
		}
	}
}
