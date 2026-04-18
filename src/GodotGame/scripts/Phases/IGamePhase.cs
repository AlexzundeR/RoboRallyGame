namespace AiTest.Godot.Phases;

using System;
using AiTest.Core.Messages;
using AiTest.Godot;

public interface IGamePhase
{
    void Initialize(IGameProvider gameProvider, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete);
    void OnEnter();
    void Update(double delta);
    void HandleInput();
    void OnExit();
}
