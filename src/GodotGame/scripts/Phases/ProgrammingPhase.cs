namespace AiTest.Godot.Phases;

using Godot;
using System;
using AiTest.Core.Engine;
using AiTest.Core.Messages;

public interface IInputService
{
	bool IsActionJustPressed(string action);
}

public class GodotInputService : IInputService
{
	public bool IsActionJustPressed(string action) => Input.IsActionJustPressed(action);
}

public class ProgrammingPhase : IGamePhase
	{
		private IGameProvider? _gameProvider;
		private IMessageDispatcher? _dispatcher;
		private Action<GamePhaseType>? _onPhaseComplete;
		private IInputService _inputService;
		private int _selectedRegister = 0;

		public int CurrentPlayerIndex => _gameProvider?.GetState().CurrentPlayerIndex ?? 0;
		public string CurrentPlayerName => _gameProvider?.GetState().Players[CurrentPlayerIndex].Name ?? "";
		public int CurrentRegister => _gameProvider?.GetState().CurrentRegister ?? 0;
		public int SelectedRegister => _selectedRegister;

		public ProgrammingPhase() : this(new GodotInputService()) { }

		public ProgrammingPhase(IInputService inputService)
		{
			_inputService = inputService;
		}

		public void Initialize(IGameProvider gameProvider, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete)
		{
			_gameProvider = gameProvider;
			_dispatcher = dispatcher;
			_onPhaseComplete = onPhaseComplete;

			dispatcher.Register<SelectCardCommand>(cmd => SelectCard(cmd.CardIndex, _selectedRegister));
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
		if (_gameProvider == null || _dispatcher == null) 
		{
			GD.Print("[ProgrammingPhase] HandleInput: runner or dispatcher is null, returning");
			return;
		}

		if (_inputService.IsActionJustPressed("ui_power_down"))
		{
			TogglePowerDown();
			return;
		}

		var player = _gameProvider.GetState().Players[CurrentPlayerIndex];
		if (player.IsPowerDown)
		{
			ConfirmPlayer();
			return;
		}

		if (CurrentRegister >= 5) 
		{
			GD.Print("[ProgrammingPhase] HandleInput: all 5 registers filled, ignoring key inputs");
			return;
		}

		if (_inputService.IsActionJustPressed("ui_left"))
		{
			_selectedRegister = Math.Max(0, _selectedRegister - 1);
			_dispatcher.Publish(new RegisterSelectionChangedEvent(_selectedRegister));
			return;
		}

		if (_inputService.IsActionJustPressed("ui_right"))
		{
			_selectedRegister = Math.Min(4, _selectedRegister + 1);
			_dispatcher.Publish(new RegisterSelectionChangedEvent(_selectedRegister));
			return;
		}

		for (int i = 1; i <= 9; i++)
		{
			string action = $"ui_{i}";
			if (_inputService.IsActionJustPressed(action))
			{
				SelectCard(i - 1, _selectedRegister);
				return;
			}
		}

		if (_inputService.IsActionJustPressed("ui_accept"))
		{
			ConfirmPlayer();
		}
	}

	public void SelectCard(int cardIndex, int registerIndex)
	{
		if (_gameProvider == null || _dispatcher == null) return;

		if (cardIndex < 9 && registerIndex < 5)
		{
			var card = _gameProvider.GetHandCard(cardIndex);
			if (card == null) return;
			
			_gameProvider.AssignCardToRegister(cardIndex, registerIndex);
			
			_dispatcher.Publish(new CardSelectedEvent(CurrentPlayerIndex, cardIndex, registerIndex));
			
			_selectedRegister = Math.Min(4, _selectedRegister + 1);
		}
	}

	public void ConfirmPlayer()
	{
		if (_gameProvider == null || _dispatcher == null) return;

		if (!_gameProvider.CanConfirmRegistration())
		{
			GD.Print("[ProgrammingPhase] ConfirmPlayer: cannot confirm - not all possible registers filled");
			return;
		}

		_dispatcher.Publish(new PlayerConfirmedEvent(CurrentPlayerIndex));
		
		var state = _gameProvider.GetState();
		var playerCount = state.Players.Count;
		if (state.CurrentPlayerIndex < playerCount - 1)
		{
			_gameProvider.SetCurrentPlayerIndex(state.CurrentPlayerIndex + 1);
			_gameProvider.SetCurrentRegister(0);
			_selectedRegister = 0;
		}
		else
		{
			_gameProvider.SetCurrentPlayerIndex(0);
			_gameProvider.SetCurrentRegister(0);
			_selectedRegister = 0;
			_onPhaseComplete?.Invoke(GamePhaseType.Execution);
		}
	}

	public void OnExit()
	{
	}

	public void TogglePowerDown()
	{
		if (_gameProvider == null || _dispatcher == null) return;

		_gameProvider.TogglePowerDown();
		var player = _gameProvider.GetCurrentPlayer();
		_dispatcher.Publish(new PowerDownToggledEvent(CurrentPlayerIndex, player?.IsPowerDown ?? false));
	}
}
