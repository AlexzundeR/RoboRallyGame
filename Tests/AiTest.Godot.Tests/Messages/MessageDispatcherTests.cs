namespace AiTest.Godot.Tests.Messages;

using AiTest.Core.Messages;
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

    private record TestEvent : IEvent;
}
