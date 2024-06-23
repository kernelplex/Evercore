using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Transactions;
using Evercore.Context;
using Evercore.Data;
using Evercore.Exceptions;
using Evercore.Monads;
using Evercore.Storage;
using Evercore.StrongTypes;
using Evercore.Tools;

namespace Evercore;

public class EventStore: IEventStore, IEventStoreContextManager
{
    private IStorageEngine _storageEngine;
    private Dictionary<string, Type> knownEventTypes = new();
    private readonly Dictionary<string, int> _aggregateTypes = new();
    private readonly Dictionary<string, int> _eventTypes = new();
    private readonly Dictionary<string, int> _agentTypes = new();
    private readonly LruCache<Agent, long> _agents;

    public EventStore(IStorageEngine storageEngine)
    {
        _storageEngine = storageEngine;
        _agents = new(1000, this.ReadAgentId); 
    }

    public EventStore RegisterEventsInAssembly(Assembly targetAssembly)
    {
        var eventImplementingTypes = GetEventImplementingTypes(targetAssembly);
        RegisterTypes(eventImplementingTypes);
        return this;
    }
    
    private IEnumerable<Type> GetEventImplementingTypes(Assembly targetAssembly)
    {
        return targetAssembly.GetTypes().Where(x => x.IsAssignableTo(typeof(IEvent)) && x is { IsAbstract: false, IsClass: true });
    }

    private void RegisterTypes(IEnumerable<Type> types)
    {
        foreach (var knownEventType in types)
        {
            var field = knownEventType.GetProperty("EventType", BindingFlags.Public | BindingFlags.Static);
            if (field is null)
            {
                throw new NullReferenceException("Property EventType not provided.");
            }

            var eventType = field.GetValue(null) as EventType;

            if (eventType is null)
            {
                throw new NamingConventionException("EventType cannot be null.", 
                    knownEventType, "EventType");
            }
            knownEventTypes.Add(eventType, knownEventType);
        }
    }
    
    public EventStore RegisterEventType<TEventType>() where TEventType : IEvent
    {
        var eventType = TEventType.EventType;
        if (eventType is null)
        {
            throw new NamingConventionException("EventType cannot be null.", 
                typeof(TEventType), "EventType");
        }
        knownEventTypes.Add(eventType, typeof(TEventType));
        return this;
    }

    public object Deserialize(EventType eventType, string data)
    {
        if (!knownEventTypes.TryGetValue(eventType, out var type))
        {
            throw new InvalidOperationException("Invalid or unknown event type.");
        }
        var result = JsonSerializer.Deserialize(data, type);

        Debug.Assert(result != null, nameof(result) + " != null");
        return result;
    }

    private async ValueTask<EventDto> ConvertToEventDto(AggregateEvent aggregateEvent, CancellationToken cancellationToken)
    {
        var aggregateTypeId = await GetAggregateTypeId(aggregateEvent.AggregateType, cancellationToken);
        var eventTypeId = await GetEventTypeId(aggregateEvent.EventType, cancellationToken);
        var agentId = await GetAgentId(aggregateEvent.Agent, cancellationToken);

        return new EventDto(aggregateTypeId, aggregateEvent.AggregateId, eventTypeId, aggregateEvent.Sequence,
            aggregateEvent.Data, agentId, aggregateEvent.EventTime);
    }

    public async Task<T> WithContext<T>(Func<IEventStoreContext, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        
        var context = new EventStoreContext(this);
        var result = await action(context, cancellationToken);
        await WriteEventsAndSnapshots(context, cancellationToken);
        
        scope.Complete();
        return result;
    }
    
    public async Task WithContext(Func<IEventStoreContext, CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        
        var context = new EventStoreContext(this);
        await action(context, cancellationToken);
        await WriteEventsAndSnapshots(context, cancellationToken);
        
        scope.Complete();
    }

    private async Task WriteEventsAndSnapshots(EventStoreContext context, CancellationToken cancellationToken)
    {
        var capturedEvents = context.GetCapturedEvents();
        var eventDtos = await Task.WhenAll(capturedEvents.Select(async x => 
            await ConvertToEventDto(x, cancellationToken)));
        await _storageEngine.StoreEvents(eventDtos, cancellationToken);

        var snapshotAggregates = context.GetAggregatesRequiringSnapshots();
        foreach (var aggregate in snapshotAggregates)
        {
            var snapshot = aggregate.TakeSnapshot();
            var snapshotDto = await ConvertToSnapshotRecord(snapshot, cancellationToken);
            await _storageEngine.SaveSnapshot(snapshotDto, cancellationToken);
        }
    }

    private async Task<SnapshotDto> ConvertToSnapshotRecord(Snapshot snapshot, CancellationToken cancellationToken)
    {
        var aggregateTypeId = await GetAggregateTypeId(snapshot.AggregateType, cancellationToken);
        return new SnapshotDto(aggregateTypeId, snapshot.AggregateId, snapshot.SnapshotVersion, snapshot.Sequence,
            snapshot.State);
    }

    public async Task WithReadonlyContext(Func<IEventStoreReadContext, CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        var context = new EventStoreContext(this);
        await action(context, cancellationToken);
    }

    public async Task<T> WithReadonlyContext<T>(Func<IEventStoreReadContext, CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        var context = new EventStoreContext(this);
        var result = await action(context, cancellationToken);
        return result;
    }

    private async Task<long> GetAgentId(Agent agent, CancellationToken cancellationToken)
    {
        return await _agents.Get(agent, cancellationToken);
    }

    private async Task<long> ReadAgentId(Agent agent, CancellationToken cancellationToken)
    {
        var agentTypeId = await GetAgentTypeId(agent.AgentType, cancellationToken);
        var agentId = await _storageEngine.GetAgentId(agentTypeId, agent.AgentKey, agent.SystemId, cancellationToken);
        return agentId;
    }

    private async ValueTask<int> GetAgentTypeId(AgentType agentType, CancellationToken cancellationToken)
    {
        if (_agentTypes.TryGetValue(agentType, out var agentId))
        {
            return agentId;
        }

        agentId = await _storageEngine.GetAgentTypeId(agentType, cancellationToken);
        _agentTypes[agentType] = agentId;
        return agentId;
    }

    private async ValueTask<int> GetEventTypeId(EventType eventType, CancellationToken cancellationToken)
    {
        if (_eventTypes.TryGetValue(eventType, out var eventTypeId))
        {
            return eventTypeId;
        }

        eventTypeId = await _storageEngine.GetEventTypeId(eventType, cancellationToken);
        _eventTypes[eventType] = eventTypeId;
        return eventTypeId;
    }

    private async ValueTask<int> GetAggregateTypeId(AggregateType aggregateType, CancellationToken cancellationToken)
    {
        if (_aggregateTypes.TryGetValue(aggregateType, out int aggregateTypeId))
        {
            return aggregateTypeId;
        }

        aggregateTypeId = await _storageEngine.GetAggregateTypeId(aggregateType, cancellationToken);
        _aggregateTypes[aggregateType] = aggregateTypeId;
        return aggregateTypeId;
    } 

    
    public async Task<long> CreateAggregate(AggregateType aggregateType, NaturalKey? naturalKey, CancellationToken cancellationToken)
    {
        var aggregateTypeId = await GetAggregateTypeId(aggregateType, cancellationToken);
        
        // TODO: Maybe we want an LRUCache for this.
        var id = await _storageEngine.CreateAggregate(aggregateTypeId, naturalKey, cancellationToken);
        return id;
    }

    public async Task<IEnumerable<AggregateEvent>> GetEvents(AggregateType aggregateType, long id, long sequence, CancellationToken cancellationToken)
    {
        var aggregateTypeId = await GetAggregateTypeId(aggregateType, cancellationToken);
        return await _storageEngine.GetAggregateEvents(aggregateTypeId, id, sequence, cancellationToken);
    }

    public async Task<Option<Snapshot>> GetSnapshot(AggregateType aggregateType, long userId, int version, 
        CancellationToken cancellationToken)
    {
        var aggregateTypeId = await GetAggregateTypeId(aggregateType, cancellationToken);
        return await _storageEngine.GetSnapshot(aggregateTypeId, userId, version, cancellationToken);
    }
}