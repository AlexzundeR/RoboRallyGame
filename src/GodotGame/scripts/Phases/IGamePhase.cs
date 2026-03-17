namespace AiTest.Godot.Phases;

using System;
using AiTest.Core.Engine;
using AiTest.Core.Messages;

public interface IGamePhase
{
    void Initialize(GameRunner runner, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete);
    void OnEnter();
    void Update(double delta);
    void HandleInput();
    void OnExit();
}
