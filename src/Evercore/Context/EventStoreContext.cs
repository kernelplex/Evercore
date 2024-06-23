using System.Text.Json;
using Evercore.Data;
using Evercore.Monads;
using Evercore.StrongTypes;

namespace Evercore.Context;

public class EventStoreContext: IEventStoreContext
{
    private readonly IEventStoreContextManager _eventStoreContextManager;
    private readonly List<AggregateEvent> _capturedEvents = [];
    private readonly HashSet<ISnapshotAggregateTraits> _aggregatesRequiringSnapshots = [];

    public EventStoreContext(IEventStoreContextManager eventStoreContextManager)
    {
        _eventStoreContextManager = eventStoreContextManager;
    }

    public async Task<T> Create<T>(NaturalKey? naturalKey = null, CancellationToken cancellationToken = default) where T : IAggregate
    {
        var aggregateType = T.AggregateType;
        var id = await _eventStoreContextManager.CreateAggregate(aggregateType, naturalKey, cancellationToken);
        return (T) T.Initialize(id);
    }


    public async Task<Option<T>> Load<T>(long id, CancellationToken cancellationToken = default) where T : IAggregate
    {
        var aggregateType = T.AggregateType;
        
        var aggregate = (T) T.Initialize(id);
        long sequence = 0;
        var aggregateLoaded = false;
        if (aggregate is ISnapshotAggregateTraits snapshotAggregate)
        {
            var version = snapshotAggregate.GetCurrentSnapshotVersion();
            var maybeSnapshot = await _eventStoreContextManager.GetSnapshot(aggregateType, id, version, cancellationToken);
            if (maybeSnapshot is Some<Snapshot> snapshot)
            {
                snapshotAggregate.ApplySnapshot(snapshot.Value);
                aggregateLoaded = true;
                sequence = snapshot.Value.Sequence;
            }
        }

        var events = await _eventStoreContextManager.GetEvents(aggregateType, id, sequence,
            cancellationToken);
        
        foreach (var currentEvent in events)
        {
            aggregateLoaded = true;
            var eventType = currentEvent.EventType;
            if (_eventStoreContextManager.Deserialize(eventType, currentEvent.Data) is IEvent @event)
            {
                aggregate.ApplyEvent(@event, currentEvent.Sequence, currentEvent.Agent, currentEvent.EventTime);
            }
            else
            {
                throw new InvalidOperationException("Deserializing event failed.");
            }
            sequence = currentEvent.Sequence;
        }

        if (!aggregateLoaded)
        {
            return new None<T>();
        }

        if (aggregate.Sequence != sequence)
        {
            throw new InvalidOperationException("Aggregate did not update the sequence appropriately.");
        }

        return new Some<T>(aggregate);
    }

    public void Apply<TEvent, TAggregate>(TEvent @event, TAggregate aggregate, Agent agent, DateTime? eventTime = null)
        where TEvent : IEvent where TAggregate : IAggregate
    {
        var dateTime = eventTime ?? DateTime.UtcNow;
        var startSequence = aggregate.Sequence;
        var endSequence = aggregate.Sequence + 1;
        aggregate.ApplyEvent(@event, endSequence, agent, dateTime);
        
        // TODO: Consider some way of forcing this or doing this internally here.
        if (aggregate.Sequence != endSequence)
        {
            throw new InvalidOperationException("Aggregate sequence was not updated.");
        }

        if (aggregate is ISnapshotAggregate snapshotAggregate)
        {
            var frequency = snapshotAggregate.GetSnapshotFrequency();
            if (frequency > 0)
            {
                var startComputedSnapshotSequence = startSequence / frequency;
                var endComputedSnapshotSequence = endSequence / frequency;
                if (startComputedSnapshotSequence != endComputedSnapshotSequence)
                {
                    _aggregatesRequiringSnapshots.Add(snapshotAggregate);
                }
            }
        }

        var data = JsonSerializer.Serialize(@event);
        _capturedEvents.Add(new AggregateEvent(TAggregate.AggregateType, aggregate.Id, TEvent.EventType, aggregate.Sequence, data,
            agent, dateTime));
    }

    public IEnumerable<AggregateEvent> GetCapturedEvents()
    {
        return _capturedEvents.ToList();
    }

    public IEnumerable<ISnapshotAggregateTraits> GetAggregatesRequiringSnapshots()
    {
        return _aggregatesRequiringSnapshots;
    }
}