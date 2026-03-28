namespace AiTest.Core.Messages;

public interface IMessageDispatcher
{
    void Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IEvent;
    void Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IEvent;
    void Publish<TEvent>(TEvent @event) where TEvent : IEvent;

    void Register<TCommand>(Action<TCommand> commandHandler) where TCommand : ICommand;
    void Unregister<TCommand>(Action<TCommand> commandHandler) where TCommand : ICommand;
    void Send<TCommand>(TCommand command) where TCommand : ICommand;
}
