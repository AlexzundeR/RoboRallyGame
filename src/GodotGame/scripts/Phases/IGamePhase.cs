namespace RoboRallyGame.Godot.Phases;

using System;
using RoboRallyGame.Core.Messages;
using RoboRallyGame.Godot;

public interface IGamePhase
{
    void Initialize(IGameProvider gameProvider, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete);
    void OnEnter();
    void Update(double delta);
    void HandleInput();
    void OnExit();
}
