using Evercore.Data;

namespace Evercore.Context;

/// <summary>
/// Represents a context publisher for interacting with an event store.
/// </summary>
public interface IEventStoreContextPublisher
{
    /// <summary>
    /// Apply an event to an aggregate using the event store context publisher.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
    /// <param name="event">The event to be applied.</param>
    /// <param name="aggregate">The aggregate to apply the event to.</param>
    /// <param name="agent">The agent associated with the event.</param>
    /// <param name="eventTime">The time of the event. If not provided, the current UTC time will be used.</param>
    public void Apply<TEvent, TAggregate>(TEvent @event, TAggregate aggregate, Agent agent, DateTime? eventTime = null)
        where TEvent : IEvent where TAggregate : IAggregate;
}