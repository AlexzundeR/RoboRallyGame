namespace AiTest.Godot.Phases;

using System;
using AiTest.Core.Engine;
using AiTest.Core.Messages;
using AiTest.Core.Entities;

public class ProgrammingPhase : IGamePhase
{
    private GameRunner? _runner;
    private IMessageDispatcher? _dispatcher;
    private Action<GamePhaseType>? _onPhaseComplete;

    public int CurrentPlayerIndex => _runner?.Engine.State.CurrentPlayerIndex ?? 0;
    public string CurrentPlayerName => _runner?.Engine.State.Players[CurrentPlayerIndex].Name ?? "";
    public int CurrentRegister => _runner?.Engine.State.CurrentRegister ?? 0;

    public void Initialize(GameRunner runner, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete)
    {
        _runner = runner;
        _dispatcher = dispatcher;
        _onPhaseComplete = onPhaseComplete;
    }

    public void OnEnter()
    {
    }

    public void Update(double delta)
    {
    }

    public void HandleInput()
    {
    }

    public void OnExit()
    {
    }
}
