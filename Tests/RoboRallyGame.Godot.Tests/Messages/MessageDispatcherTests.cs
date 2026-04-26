namespace RoboRallyGame.Godot.Tests.Messages;

using RoboRallyGame.Core.Messages;
using Xunit;

public class MessageDispatcherTests
{
    [Fact]
    public void Subscribe_ShouldReceivePublishedEvent()
    {
        var dispatcher = new MessageDispatcher();
        var received = false;

        dispatcher.Subscribe<TestEvent>(e => received = true);
        dispatcher.Publish(new TestEvent());

        Assert.True(received);
    }

    [Fact]
    public void Publish_MultipleSubscribers_ShouldNotifyAll()
    {
        var dispatcher = new MessageDispatcher();
        var count = 0;

        dispatcher.Subscribe<TestEvent>(_ => count++);
        dispatcher.Subscribe<TestEvent>(_ => count++);
        dispatcher.Publish(new TestEvent());

        Assert.Equal(2, count);
    }

    [Fact]
    public void Unsubscribe_ShouldStopReceivingEvents()
    {
        var dispatcher = new MessageDispatcher();
        var count = 0;
        Action<TestEvent> handler = _ => count++;
        dispatcher.Subscribe(handler);
        dispatcher.Publish(new TestEvent());

        dispatcher.Unsubscribe(handler);
        dispatcher.Publish(new TestEvent());

        Assert.Equal(1, count);
    }

    [Fact]
    public void Publish_NoSubscribers_ShouldNotThrow()
    {
        var dispatcher = new MessageDispatcher();
        dispatcher.Publish(new TestEvent());
    }

    [Fact]
    public void Publish_WithEventData_ShouldPassCorrectData()
    {
        var dispatcher = new MessageDispatcher();
        CardSelectedEvent? received = null;

        dispatcher.Subscribe<CardSelectedEvent>(e => received = e);
        dispatcher.Publish(new CardSelectedEvent(2, 5, 3));

        Assert.NotNull(received);
        Assert.Equal(2, received.PlayerIndex);
        Assert.Equal(5, received.CardIndex);
        Assert.Equal(3, received.Register);
    }

    [Fact]
    public void Send_Command_ShouldExecuteHandler()
    {
        var dispatcher = new MessageDispatcher();
        var executed = false;

        dispatcher.Register<SelectCardCommand>(_ => executed = true);
        dispatcher.Send(new SelectCardCommand(0));

        Assert.True(executed);
    }

    [Fact]
    public void Send_NoHandlerRegistered_ShouldNotThrow()
    {
        var dispatcher = new MessageDispatcher();
        dispatcher.Send(new SelectCardCommand(0));
    }

    [Fact]
    public void Send_SelectCardCommand_ShouldExecuteHandler()
    {
        var dispatcher = new MessageDispatcher();
        SelectCardCommand? received = null;

        dispatcher.Register<SelectCardCommand>(cmd => received = cmd);
        dispatcher.Send(new SelectCardCommand(3));

        Assert.NotNull(received);
        Assert.Equal(3, received.CardIndex);
    }

    [Fact]
    public void Send_ConfirmPlayerCommand_ShouldExecuteHandler()
    {
        var dispatcher = new MessageDispatcher();
        var executed = false;

        dispatcher.Register<ConfirmPlayerCommand>(_ => executed = true);
        dispatcher.Send(new ConfirmPlayerCommand());

        Assert.True(executed);
    }

    [Fact]
    public void Send_ExecuteRegisterCommand_ShouldExecuteHandler()
    {
        var dispatcher = new MessageDispatcher();
        var executed = false;

        dispatcher.Register<ExecuteRegisterCommand>(_ => executed = true);
        dispatcher.Send(new ExecuteRegisterCommand());

        Assert.True(executed);
    }

    [Fact]
    public void Publish_CardSelectedEvent_ShouldNotifySubscriber()
    {
        var dispatcher = new MessageDispatcher();
        var events = new List<CardSelectedEvent>();

        dispatcher.Subscribe<CardSelectedEvent>(e => events.Add(e));
        dispatcher.Publish(new CardSelectedEvent(0, 5, 2));

        Assert.Single(events);
        Assert.Equal(0, events[0].PlayerIndex);
        Assert.Equal(5, events[0].CardIndex);
        Assert.Equal(2, events[0].Register);
    }

    [Fact]
    public void Publish_PlayerConfirmedEvent_ShouldNotifySubscriber()
    {
        var dispatcher = new MessageDispatcher();
        var events = new List<PlayerConfirmedEvent>();

        dispatcher.Subscribe<PlayerConfirmedEvent>(e => events.Add(e));
        dispatcher.Publish(new PlayerConfirmedEvent(1));

        Assert.Single(events);
        Assert.Equal(1, events[0].PlayerIndex);
    }

    [Fact]
    public void Publish_RegisterExecutedEvent_ShouldNotifySubscriber()
    {
        var dispatcher = new MessageDispatcher();
        var events = new List<RegisterExecutedEvent>();

        dispatcher.Subscribe<RegisterExecutedEvent>(e => events.Add(e));
        dispatcher.Publish(new RegisterExecutedEvent(3, 2));

        Assert.Single(events);
        Assert.Equal(3, events[0].Register);
        Assert.Equal(2, events[0].PlayerIndex);
    }

    [Fact]
    public void Publish_PhaseChangedEvent_ShouldNotifySubscriber()
    {
        var dispatcher = new MessageDispatcher();
        var events = new List<PhaseChangedEvent>();

        dispatcher.Subscribe<PhaseChangedEvent>(e => events.Add(e));
        dispatcher.Publish(new PhaseChangedEvent(GamePhaseType.Execution, 2));

        Assert.Single(events);
        Assert.Equal(GamePhaseType.Execution, events[0].Phase);
        Assert.Equal(2, events[0].Round);
    }

    [Fact]
    public void Publish_RegisterSelectionChangedEvent_ShouldNotifySubscriber()
    {
        var dispatcher = new MessageDispatcher();
        var events = new List<RegisterSelectionChangedEvent>();

        dispatcher.Subscribe<RegisterSelectionChangedEvent>(e => events.Add(e));
        dispatcher.Publish(new RegisterSelectionChangedEvent(2));

        Assert.Single(events);
        Assert.Equal(2, events[0].SelectedRegister);
    }

    [Fact]
    public void Unregister_ShouldStopExecutingHandler()
    {
        var dispatcher = new MessageDispatcher();
        var count = 0;
        Action<ConfirmPlayerCommand> handler = _ => count++;
        dispatcher.Register(handler);
        dispatcher.Send(new ConfirmPlayerCommand());

        dispatcher.Unregister(handler);
        dispatcher.Send(new ConfirmPlayerCommand());

        Assert.Equal(1, count);
    }

    [Fact]
    public void MultipleCommandTypes_CanBeRegisteredIndependently()
    {
        var dispatcher = new MessageDispatcher();
        var selectCount = 0;
        var confirmCount = 0;
        var executeCount = 0;

        dispatcher.Register<SelectCardCommand>(_ => selectCount++);
        dispatcher.Register<ConfirmPlayerCommand>(_ => confirmCount++);
        dispatcher.Register<ExecuteRegisterCommand>(_ => executeCount++);

        dispatcher.Send(new SelectCardCommand(0));
        dispatcher.Send(new ConfirmPlayerCommand());
        dispatcher.Send(new ExecuteRegisterCommand());

        Assert.Equal(1, selectCount);
        Assert.Equal(1, confirmCount);
        Assert.Equal(1, executeCount);
    }

    private record TestEvent : IEvent;
}
