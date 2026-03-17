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

        dispatcher.Register<SelectCardCommand>(cmd => SelectCard(cmd.CardIndex));
        dispatcher.Register<ConfirmPlayerCommand>(_ => ConfirmPlayer());
    }

    public void OnEnter()
    {
    }

    public void Update(double delta)
    {
    }

    public void HandleInput()
    {
        if (_runner == null || _dispatcher == null) return;

        for (int i = 1; i <= 9; i++)
        {
            string action = $"ui_{i}";
            if (Input.IsActionJustPressed(action))
            {
                SelectCard(i - 1);
                return;
            }
        }

        if (Input.IsActionJustPressed("ui_accept"))
        {
            ConfirmPlayer();
        }
    }

    public void SelectCard(int cardIndex)
    {
        if (_runner == null || _dispatcher == null) return;

        var player = _runner.Engine.State.Players[CurrentPlayerIndex];
        
        if (cardIndex < player.Hand.Count)
        {
            var card = player.Hand[cardIndex];
            
            player.Registers[CurrentRegister] = card;
            player.Hand.RemoveAt(cardIndex);
            
            _dispatcher.Publish(new CardSelectedEvent(CurrentPlayerIndex, cardIndex, CurrentRegister));
            
            if (CurrentRegister < 4)
            {
                _runner.Engine.State.CurrentRegister++;
            }
        }
    }

    public void ConfirmPlayer()
    {
        if (_runner == null || _dispatcher == null) return;

        _dispatcher.Publish(new PlayerConfirmedEvent(CurrentPlayerIndex));
        
        if (_runner.Engine.State.CurrentPlayerIndex < _runner.Engine.State.Players.Count - 1)
        {
            _runner.Engine.State.CurrentPlayerIndex++;
            _runner.Engine.State.CurrentRegister = 0;
        }
        else
        {
            _runner.Engine.State.CurrentPlayerIndex = 0;
            _runner.Engine.State.CurrentRegister = 0;
            _onPhaseComplete?.Invoke(GamePhaseType.Execution);
        }
    }

    public void SimulateKeyPress(int keyNumber)
    {
        if (keyNumber >= 1 && keyNumber <= 9)
        {
            SelectCard(keyNumber - 1);
        }
    }

    public void SimulateConfirm()
    {
        ConfirmPlayer();
    }

    public void OnExit()
    {
    }
}
