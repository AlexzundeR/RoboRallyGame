namespace AiTest.Godot.Phases;

using Godot;
using System;
using AiTest.Core.Engine;
using AiTest.Core.Messages;

public class ExecutionPhase : IGamePhase
{
	private GameRunner? _runner;
	private IMessageDispatcher? _dispatcher;
	private Action<GamePhaseType>? _onPhaseComplete;
	private IInputService _inputService;
	private bool _registerExecuted;
	private bool _inputHandled;

	public int CurrentPlayerIndex => _runner?.Engine.State.CurrentPlayerIndex ?? 0;
	public string CurrentPlayerName => _runner?.Engine.State.Players[CurrentPlayerIndex].Name ?? "";
	public int CurrentRegister => _runner?.Engine.State.CurrentRegister ?? 0;
	public int CurrentPriority => _runner?.Engine.State.Players[CurrentPlayerIndex].Registers[CurrentRegister]?.Card.Priority ?? 0;

	public ExecutionPhase() : this(new GodotInputService()) { }

	public ExecutionPhase(IInputService inputService)
	{
		_inputService = inputService;
	}

	public void Initialize(GameRunner runner, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete)
	{
		_runner = runner;
		_dispatcher = dispatcher;
		_onPhaseComplete = onPhaseComplete;

		dispatcher.Register<ExecuteRegisterCommand>(_ => ExecuteCurrentRegister());
	}

	public void OnEnter()
	{
		_registerExecuted = false;
		_inputHandled = false;
		if (_runner != null)
		{
			_runner.Engine.State.CurrentRegister = 0;
			_runner.Engine.State.CurrentPlayerIndex = 0;
		}
	}

	public void Update(double delta)
	{
	}

	public void HandleInput()
	{
		if (_inputHandled) return;
		
		if (_runner == null || _dispatcher == null) 
		{
			GD.Print("[ExecutionPhase] HandleInput: runner or dispatcher is null, returning");
			return;
		}

		if (_inputService.IsActionJustPressed("ui_accept") || _inputService.IsActionJustPressed("ui_execute"))
		{
			_inputHandled = true;
			ExecuteCurrentRegister();
		}
	}

	public void ExecuteRegister()
	{
		ExecuteCurrentRegister();
	}

	private void ExecuteCurrentRegister()
	{
		if (_runner == null || _dispatcher == null) 
		{
			GD.Print("[ExecutionPhase] ExecuteCurrentRegister: runner or dispatcher is null, returning");
			return;
		}

		var state = _runner.Engine.State;

		if (state.CurrentRegister >= 5)
		{
			CompleteExecutionPhase();
			return;
		}

		_runner.Engine.ExecuteRegister(state.CurrentRegister);

		_dispatcher.Publish(new RegisterExecutedEvent(state.CurrentRegister, state.CurrentPlayerIndex));

		state.CurrentRegister++;
		
		_inputHandled = false;

		if (state.CurrentRegister >= 5)
		{
			CompleteExecutionPhase();
		}

		_registerExecuted = true;
	}

	private void CompleteExecutionPhase()
	{
		if (_runner == null || _dispatcher == null) return;

		var state = _runner.Engine.State;

		if (state.CurrentPlayerIndex < state.Players.Count - 1)
		{
			state.CurrentPlayerIndex++;
			state.CurrentRegister = 0;
			_dispatcher.Publish(new PhaseChangedEvent(GamePhaseType.Execution, state.Round));
		}
		else
		{
			foreach (var player in state.Players)
			{
				player.ClearRegisters();
			}
			
			state.CurrentPlayerIndex = 0;
			state.CurrentRegister = 0;
			state.Round++;
			_runner.Engine.NextRound();
			_dispatcher.Publish(new PhaseChangedEvent(GamePhaseType.Programming, state.Round));
			_onPhaseComplete?.Invoke(GamePhaseType.Programming);
		}
	}

	public void OnExit()
	{
	}
}
