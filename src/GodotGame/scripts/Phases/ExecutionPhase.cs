namespace AiTest.Godot.Phases;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using AiTest.Core.Engine;
using AiTest.Core.Messages;
using AiTest.Core.Models;
using AiTest.Godot;
using AiTest.Godot.Animations;

public class ExecutionPhase : IGamePhase
{
	private GameRunner? _runner;
	private IMessageDispatcher? _dispatcher;
	private Action<GamePhaseType>? _onPhaseComplete;
	private IAnimationService? _animationService;
	private RobotRenderer? _robotLayer;
	private RichTextLabel? _actionLog;
	private IInputService _inputService;
	private bool _inputHandled;

	private readonly List<Animation> _animations = new();
	private int _currentRegisterLog = -1;

	public int CurrentPlayerIndex => _runner?.Engine.State.CurrentPlayerIndex ?? 0;
	public string CurrentPlayerName => _runner?.Engine.State.Players[CurrentPlayerIndex].Name ?? "";
	public int CurrentRegister => _runner?.Engine.State.CurrentRegister ?? 0;
	public int CurrentPriority => _runner?.Engine.State.Players[CurrentPlayerIndex].Registers[CurrentRegister]?.Card.Priority ?? 0;

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

	public void SetActionLog(RichTextLabel logLabel)
	{
		_actionLog = logLabel;
	}

	public void Initialize(GameRunner runner, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete)
	{
		_runner = runner;
		_dispatcher = dispatcher;
		_onPhaseComplete = onPhaseComplete;

		dispatcher.Register<ExecuteRegisterCommand>(_ => StartRegisterExecution());

		var engine = runner.Engine;
		engine.OnMove += OnMove;
		engine.OnRotate += OnRotate;
		engine.OnConveyorMove += OnConveyorMove;
		engine.OnLaserFire += OnLaserFire;
		engine.OnDamage += OnDamage;
		engine.OnCheckpoint += OnCheckpoint;
		engine.OnRobotDestroyed += OnRobotDestroyed;
		engine.OnRobotReboot += OnRobotReboot;
		engine.OnRobotRepaired += OnRobotRepaired;
		engine.OnLifeTokenLost += OnLifeTokenLost;
		engine.OnAllRegistersComplete += OnAllRegistersComplete;
	}

	private void OnAllRegistersComplete()
	{
		CompleteExecutionPhase();
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

	private string GetPlayerName(int playerId)
	{
		return _runner?.Engine.State.Players.FirstOrDefault(p => p.Id == playerId)?.Name ?? $"Player {playerId}";
	}

	private string GetPlayerColor(int playerId)
	{
		return playerId == 1 ? "cyan" : "orange";
	}

	private void Log(string message)
	{
		_actionLog?.AppendText(message + "\n");
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
		if (_runner != null)
		{
			_runner.Engine.State.CurrentRegister = 0;
			_runner.Engine.State.CurrentPlayerIndex = 0;
		}
	}

	public void Update(double delta) { }

	public void HandleInput()
	{
		if (_inputHandled) return;
		
		if (_runner == null || _dispatcher == null) return;

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
		if (_runner == null || _dispatcher == null) return;

		var state = _runner.Engine.State;

		if (state.CurrentRegister >= 5)
		{
			CompleteExecutionPhase();
			return;
		}

		var player = state.Players[state.CurrentPlayerIndex];
		if (player.Robot.IsPowerDown)
		{
			Log($"[color=gray]{player.Name} is powered down, skipping all registers[/color]");
			CompleteCurrentRegister();
			return;
		}

		if (player.GetRegister(state.CurrentRegister) == null)
		{
			var anyPlayerHasCard = state.Players.Any(p => p.GetRegister(state.CurrentRegister) != null);
			if (!anyPlayerHasCard)
			{
				Log($"[color=gray]Register {state.CurrentRegister + 1} empty for all players, skipping[/color]");
				CompleteCurrentRegister();
				return;
			}
		}

		if (_currentRegisterLog != state.CurrentRegister)
		{
			_currentRegisterLog = state.CurrentRegister;
			Log("");
			Log($"[b]--- Register {state.CurrentRegister + 1} ---[/b]");
		}

		_runner.Engine.BeginRegisterExecution(state.CurrentRegister);
		ExecuteNextStepAsync();
	}

	private async void ExecuteNextStepAsync()
	{
		if (_runner == null) return;

		_animations.Clear();

		var result = _runner.Engine.TryExecuteNextStep();
		
		if (result == StepResult.Completed)
		{
			CompleteCurrentRegister();
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

	private void CompleteCurrentRegister()
	{
		if (_runner == null) return;

		var state = _runner.Engine.State;
		_dispatcher?.Publish(new RegisterExecutedEvent(state.CurrentRegister, state.CurrentPlayerIndex));
		Log($"[color=green]Register {state.CurrentRegister + 1} complete[/color]");

		_runner.Engine.NextRegister();
		_inputHandled = false;
	}

	private void CompleteExecutionPhase()
	{
		if (_runner == null || _dispatcher == null) return;

		var state = _runner.Engine.State;
		
		_runner.Engine.NextRound();
		Log("");
		Log("[color=green][b]=== Round Complete ===[/b][/color]");
		
		_dispatcher.Publish(new RegistersClearedEvent());
		_dispatcher.Publish(new PhaseChangedEvent(GamePhaseType.Programming, state.Round));
		_onPhaseComplete?.Invoke(GamePhaseType.Programming);
	}

	public void OnExit() { }

	private Robot? GetRobot(int playerId)
	{
		if (_robotLayer == null) return null;
		return _robotLayer.GetRobot(playerId);
	}
}
