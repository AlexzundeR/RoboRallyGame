namespace AiTest.Godot.Phases;

using System;
using AiTest.Core.Engine;
using AiTest.Core.Messages;

public class ExecutionPhase : IGamePhase
{
    private GameRunner? _runner;
    private IMessageDispatcher? _dispatcher;
    private Action<GamePhaseType>? _onPhaseComplete;
    private bool _registerExecuted;

    public int CurrentPlayerIndex => _runner?.Engine.State.CurrentPlayerIndex ?? 0;
    public string CurrentPlayerName => _runner?.Engine.State.Players[CurrentPlayerIndex].Name ?? "";
    public int CurrentRegister => _runner?.Engine.State.CurrentRegister ?? 0;
    public int CurrentPriority => _runner?.Engine.State.Players[CurrentPlayerIndex].Registers[CurrentRegister]?.Priority ?? 0;

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
        if (_runner == null || _dispatcher == null) return;

        if (Input.IsActionJustPressed("ui_accept") || Input.IsActionJustPressed("ui_execute"))
        {
            ExecuteCurrentRegister();
        }
    }

    public void ExecuteRegister()
    {
        ExecuteCurrentRegister();
    }

    private void ExecuteCurrentRegister()
    {
        if (_runner == null || _dispatcher == null) return;

        var state = _runner.Engine.State;

        if (state.CurrentRegister >= 5)
        {
            CompleteExecutionPhase();
            return;
        }

        _runner.Engine.ExecuteRegister(state.CurrentRegister);

        _dispatcher.Publish(new RegisterExecutedEvent(state.CurrentRegister, state.CurrentPlayerIndex));

        state.CurrentRegister++;

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
