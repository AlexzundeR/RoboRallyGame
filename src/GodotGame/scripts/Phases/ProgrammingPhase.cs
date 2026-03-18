namespace AiTest.Godot.Phases;

using System;
using AiTest.Core.Engine;
using AiTest.Core.Messages;
using AiTest.Core.Entities;

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
	private GameRunner? _runner;
	private IMessageDispatcher? _dispatcher;
	private Action<GamePhaseType>? _onPhaseComplete;
	private IInputService _inputService;
	private int _selectedRegister = 0;

	public int CurrentPlayerIndex => _runner?.Engine.State.CurrentPlayerIndex ?? 0;
	public string CurrentPlayerName => _runner?.Engine.State.Players[CurrentPlayerIndex].Name ?? "";
	public int CurrentRegister => _runner?.Engine.State.CurrentRegister ?? 0;
	public int SelectedRegister => _selectedRegister;

	public ProgrammingPhase() : this(new GodotInputService()) { }

	public ProgrammingPhase(IInputService inputService)
	{
		_inputService = inputService;
	}

	public void Initialize(GameRunner runner, IMessageDispatcher dispatcher, Action<GamePhaseType> onPhaseComplete)
	{
		_runner = runner;
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
		if (_runner == null || _dispatcher == null) 
		{
			GD.Print("[ProgrammingPhase] HandleInput: runner or dispatcher is null, returning");
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
		if (_runner == null || _dispatcher == null) return;

		var player = _runner.Engine.State.Players[CurrentPlayerIndex];
		
		if (cardIndex < player.Hand.Count && registerIndex < 5)
		{
			var card = player.Hand[cardIndex];
			player.AssignCardToRegister(card, cardIndex, registerIndex);
			
			_dispatcher.Publish(new CardSelectedEvent(CurrentPlayerIndex, cardIndex, registerIndex));
			
			_selectedRegister = Math.Min(4, _selectedRegister + 1);
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
			_selectedRegister = 0;
		}
		else
		{
			_runner.Engine.State.CurrentPlayerIndex = 0;
			_runner.Engine.State.CurrentRegister = 0;
			_selectedRegister = 0;
			_onPhaseComplete?.Invoke(GamePhaseType.Execution);
		}
	}

	public void OnExit()
	{
	}
}
