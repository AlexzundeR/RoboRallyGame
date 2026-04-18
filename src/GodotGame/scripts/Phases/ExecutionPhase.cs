namespace AiTest.Godot.Phases;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using AiTest.Core.Messages;
using AiTest.Core.Models;
using AiTest.Core.Upgrades;
using AiTest.Godot;
using AiTest.Godot.Animations;

public class ExecutionPhase : IGamePhase
{
	private IGameProvider? _gameProvider;
	private IMessageDispatcher? _dispatcher;
	private Action<GamePhaseType>? _onPhaseComplete;
	private IAnimationService? _animationService;
	private RobotRenderer? _robotLayer;
	private Action<string>? _logAction;
	private IInputService _inputService;
	private bool _inputHandled;

	private readonly List<Animation> _animations = new();
	private int _currentRegisterLog = -1;

	public int CurrentPlayerIndex => _gameProvider?.GetState().CurrentPlayerIndex ?? 0;
	public string CurrentPlayerName => _gameProvider?.GetState().Players[CurrentPlayerIndex].Name ?? "";
	public int CurrentRegister => _gameProvider?.GetState().CurrentRegister ?? 0;
	public int CurrentPriority
	{
		get
		{
			if (_gameProvider == null) return 0;
			var state = _gameProvider.GetState();
			var player = state.Players[CurrentPlayerIndex];
			if (player.LockedRegisters > CurrentRegister) return 0;
			var register = player.Registers[CurrentRegister];
			return register?.Card.Priority ?? 0;
		}
	}

	public ExecutionPhase() : this(new GodotInputService()) { }

	public ExecutionPhase(IInputService inputService)
	{
		_inputService = inputService;
	}

	public void SetAnimationService(IAnimationService service)
	{
		_animationService = service;
	}

	public void SetRobotLayer(RobotRenderer robotLayer)
	{
		_robotLayer = robotLayer;
	}

	public void SetLogAction(Action<string> logAction)
	{
		_logAction = logAction;
	}

	public void Initialize(IGameProvider gameProvider, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete)
	{
		_gameProvider = gameProvider;
		_dispatcher = dispatcher;
		_onPhaseComplete = onPhaseComplete;

		dispatcher.Register<ExecuteRegisterCommand>(_ => StartRegisterExecution());
		dispatcher.Subscribe<PhaseExecutionCompleteEvent>(OnPhaseExecutionComplete);
		dispatcher.Subscribe<AllRegistersCompleteEvent>(_ => CompleteExecutionPhase());
		dispatcher.Subscribe<LifeTokenLostEvent>(OnLifeTokenLost);
		dispatcher.Subscribe<RobotRebootEvent>(OnRobotReboot);
		dispatcher.Subscribe<RobotRepairedEvent>(OnRobotRepaired);
		dispatcher.Subscribe<UpgradeAcquiredEvent>(OnUpgradeAcquired);
		dispatcher.Subscribe<MoveEvent>(OnMove);
		dispatcher.Subscribe<RotateEvent>(OnRotate);
		dispatcher.Subscribe<ConveyorMoveEvent>(OnConveyorMove);
		dispatcher.Subscribe<LaserFireEvent>(OnLaserFire);
		dispatcher.Subscribe<DamageEvent>(OnDamage);
		dispatcher.Subscribe<CheckpointEvent>(OnCheckpoint);
		dispatcher.Subscribe<RobotDestroyedEvent>(OnRobotDestroyed);
	}

	private void OnPhaseExecutionComplete(PhaseExecutionCompleteEvent e)
	{
		if (_gameProvider == null) return;
		_animations.Clear();
		Log($"[color=green]Register {e.Register + 1} complete[/color]");
		_gameProvider.NextRegister();
		_inputHandled = false;
	}

	private void OnLifeTokenLost(LifeTokenLostEvent e)
	{
		Log($"[color=red]{GetPlayerName(e.PlayerId)} lost a life token! ({e.RemainingTokens} remaining)[/color]");
	}

	private void OnRobotReboot(RobotRebootEvent e)
	{
		Log($"[color=orange]Robot {GetPlayerName(e.PlayerId)} rebooted at ({e.SpawnPosition.X},{e.SpawnPosition.Y})[/color]");
	}

	private void OnRobotRepaired(RobotRepairedEvent e)
	{
		var name = GetPlayerName(e.PlayerId);
		Log($"[color=green]{name} repaired {e.RepairAmount} damage (now {e.NewDamage})[/color]");
	}

	private void OnUpgradeAcquired(UpgradeAcquiredEvent e)
	{
		var name = GetPlayerName(e.PlayerId);
		var upgrade = UpgradeCatalog.Get(e.UpgradeId);
		var upgradeName = upgrade?.Name ?? e.UpgradeId;
		Log($"[color=yellow]{name} acquired upgrade: {upgradeName}[/color]");
	}

	private string GetPlayerName(int playerId)
	{
		var player = _gameProvider?.GetPlayer(playerId);
		return player?.Name ?? $"Player {playerId}";
	}

	private string GetPlayerColor(int playerId)
	{
		return playerId == 1 ? "cyan" : "orange";
	}

	private void Log(string message)
	{
		_logAction?.Invoke(message);
	}

	private void OnMove(MoveEvent e)
	{
		var color = GetPlayerColor(e.PlayerId);
		var name = GetPlayerName(e.PlayerId);
		Log($"[color={color}]{name}[/color] moves ({e.From.X},{e.From.Y}) → ({e.To.X},{e.To.Y})");

		var robot = GetRobot(e.PlayerId);
		if (robot != null)
		{
			_animations.Add(new MoveAnimation
			{
				Robot = robot,
				From = e.From,
				To = e.To
			});
		}
	}

	private void OnRotate(RotateEvent e)
	{
		var color = GetPlayerColor(e.PlayerId);
		var name = GetPlayerName(e.PlayerId);
		var fromStr = e.From.ToString();
		var toStr = e.To.ToString();
		Log($"[color={color}]{name}[/color] rotates {fromStr} → {toStr}");

		var robot = GetRobot(e.PlayerId);
		if (robot != null)
		{
			_animations.Add(new RotateAnimation
			{
				Robot = robot,
				From = e.From,
				To = e.To
			});
		}
	}

	private void OnConveyorMove(ConveyorMoveEvent e)
	{
		var color = GetPlayerColor(e.PlayerId);
		var name = GetPlayerName(e.PlayerId);
		Log($"[color={color}]{name}[/color] pushed by conveyor → ({e.To.X},{e.To.Y})");

		var robot = GetRobot(e.PlayerId);
		if (robot != null)
		{
			_animations.Add(new ConveyorAnimation
			{
				Robot = robot,
				From = e.From,
				To = e.To
			});
		}
	}

	private void OnLaserFire(LaserFireEvent e)
	{
		var color = GetPlayerColor(e.SourceId);
		var name = GetPlayerName(e.SourceId);
		var hitMsg = e.HitPlayerId.HasValue 
			? $" hits [color=red]{GetPlayerName(e.HitPlayerId.Value)}[/color]" 
			: " misses";
		Log($"[color={color}]{name}[/color] fires laser{hitMsg}");

		_animations.Add(new LaserAnimation
		{
			Source = e.Source,
			Direction = e.Direction,
			Path = e.Path
		});
	}

	private void OnDamage(DamageEvent e)
	{
		var color = GetPlayerColor(e.PlayerId);
		var name = GetPlayerName(e.PlayerId);
		Log($"[color=red]{name}[/color] takes {e.Damage} damage (HP: {e.NewHealth})");
	}

	private void OnCheckpoint(CheckpointEvent e)
	{
		var color = GetPlayerColor(e.PlayerId);
		var name = GetPlayerName(e.PlayerId);
		Log($"[color=green]{name}[/color] reached checkpoint {e.CheckpointNumber}/{e.TotalCheckpoints}!");
	}

	private void OnRobotDestroyed(RobotDestroyedEvent e)
	{
		var color = GetPlayerColor(e.PlayerId);
		var name = GetPlayerName(e.PlayerId);
		Log($"[color=red][b]{name} ROBOT DESTROYED![/b][/color]");
	}

	public void OnEnter()
	{
		_inputHandled = false;
		_currentRegisterLog = -1;
		if (_gameProvider != null)
		{
			_gameProvider.SetCurrentRegister(0);
			_gameProvider.SetCurrentPlayerIndex(0);
		}
	}

	public void Update(double delta) { }

	public void HandleInput()
	{
		if (_inputHandled) return;
		
		if (_gameProvider == null || _dispatcher == null) return;

		if (_inputService.IsActionJustPressed("ui_accept") || _inputService.IsActionJustPressed("ui_execute"))
		{
			_inputHandled = true;
			StartRegisterExecution();
		}
	}

	public void ExecuteRegister()
	{
		StartRegisterExecution();
	}

	private void StartRegisterExecution()
	{
		if (_gameProvider == null || _dispatcher == null) return;

		ExecuteNextStepAsync();
	}

	private async void ExecuteNextStepAsync()
	{
		if (_gameProvider == null) return;

		_animations.Clear();

		var result = _gameProvider.TryExecuteNextStep();
		
		if (result == Core.Engine.StepResult.Completed)
		{
			return;
		}

		if (_animationService != null && _animations.Count > 0)
		{
			await _animationService.PlayAndWait(_animations);
		}
		
		OnAnimationsComplete();
	}

	private void OnAnimationsComplete()
	{
		ExecuteNextStepAsync();
	}

	private void CompleteExecutionPhase()
	{
		if (_gameProvider == null || _dispatcher == null) return;

		var state = _gameProvider.GetState();
		
		_gameProvider.NextRound();
		Log("");
		Log("[color=green][b]=== Round Complete ===[/b][/color]");
		
		_dispatcher.Publish(new RegistersClearedEvent());
		_dispatcher.Publish(new PhaseChangedEvent(GamePhaseType.Programming, state.Round));
		_onPhaseComplete?.Invoke(GamePhaseType.Programming);
	}

	public void OnExit() { }

	private RobotNode? GetRobot(int playerId)
	{
		if (_robotLayer == null) return null;
		return _robotLayer.GetRobot(playerId);
	}
}
