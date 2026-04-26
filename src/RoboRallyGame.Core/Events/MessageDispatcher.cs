namespace RoboRallyGame.Core.Messages;

using System.Collections.Generic;

public class MessageDispatcher : IMessageDispatcher
{
    private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();
    private readonly Dictionary<Type, Delegate> _commandHandlers = new();

    public void Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        if (!_eventHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers = new List<Delegate>();
            _eventHandlers[eventType] = handlers;
        }
        handlers.Add(eventHandler);
    }

    public void Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        if (_eventHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(eventHandler);
        }
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        if (!_eventHandlers.TryGetValue(eventType, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers)
        {
            if (handler is Action<TEvent> action)
            {
                action(@event);
            }
            else
            {
                handler.DynamicInvoke(@event);
            }
        }
    }

    public void Register<TCommand>(Action<TCommand> commandHandler) where TCommand : ICommand
    {
        var commandType = typeof(TCommand);
        _commandHandlers[commandType] = commandHandler;
    }

    public void Unregister<TCommand>(Action<TCommand> commandHandler) where TCommand : ICommand
    {
        _commandHandlers.Remove(typeof(TCommand));
    }

    public void Send<TCommand>(TCommand command) where TCommand : ICommand
    {
        var commandType = typeof(TCommand);
        if (_commandHandlers.TryGetValue(commandType, out var handler))
        {
            if (handler is Action<TCommand> action)
            {
                action(command);
            }
            else
            {
                handler.DynamicInvoke(command);
            }
        }
    }
}
