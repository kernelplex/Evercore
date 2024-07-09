using Evercore.Data;
using Evercore.Exceptions;
using Evercore.StrongTypes;
using KernelPlex.Monads;
using KernelPlex.Tools.Monads.Options;

namespace Evercore.Storage;

public class TransientMemoryStorageEngine: IStorageEngine
{
    private long _aggregateCounter;
    private readonly TwoWayLookup<AggregateType, int> _aggregateTypes = new ();
    private readonly TwoWayLookup<EventType, int> _eventTypes = new ();
    private readonly TwoWayLookup<AgentType, int> _agentTypes = new ();
    private readonly TwoWayLookup<AgentDto, long> _agents = new();
    
    
    private readonly List<AggregateDto> _aggregates = [];
    private readonly Dictionary<long, long> _aggregateSequenceMap = new();
    private readonly List<EventDto> _events = [];
    private readonly Dictionary<(int aggregateTypeId, long aggregateId), SnapshotDto> _snapshotDictionary = new ();

    public async Task<int> GetAggregateTypeId(AggregateType aggregateType, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return _aggregateTypes.GetByKey(aggregateType);
    }
    
    public async Task<int> GetEventTypeId(EventType eventType, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return _eventTypes.GetByKey(eventType);
    }

    public async Task<long> GetAgentId(int agentTypeId, AgentKey? agentKey, long? systemId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return _agents.GetByKey(new AgentDto(agentTypeId, systemId, agentKey));
    }

    public async Task<int> GetAgentTypeId(AgentType agentType, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return _agentTypes.GetByKey(agentType);
    }

    public async Task StoreEvents(IEnumerable<EventDto> eventDtos, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        foreach (var @event in eventDtos)
        {
            if (!_aggregateSequenceMap.TryGetValue(@event.AggregateId, out var currentSequence))
            {
                currentSequence = 0;
                _aggregateSequenceMap[@event.AggregateId] = currentSequence;
            }

            if (@event.Sequence != currentSequence + 1)
            {
                throw new AggregateSequenceException("Aggregate sequence integrity error.", @event.AggregateTypeId,
                    @event.AggregateId, @event.Sequence);
            }
            _events.Add(@event);
            _aggregateSequenceMap[@event.AggregateId] = @event.Sequence;
        }
    }

    public async Task<long> CreateAggregate(int aggregateTypeId, NaturalKey? naturalKey, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        if (naturalKey is null)
        {
            return ++_aggregateCounter;
        }
        
        var foundAggregate = _aggregates
            .FirstOrDefault(x => x.AggregateTypeId == aggregateTypeId && x?.NaturalKey == naturalKey);
        if (foundAggregate is not null) {
            throw new DuplicateKeyException("Aggregate with this key already exists.", aggregateTypeId, naturalKey);
        }

        var dto = new AggregateDto(++_aggregateCounter, aggregateTypeId, naturalKey);
        _aggregates.Add(dto);
        return dto.Id;
    }

    public async Task<IEnumerable<AggregateEvent>> GetAggregateEvents(int aggregateTypeId, long aggregateId,
        long minSequence, CancellationToken cancellationToken, long? maxSequence)
    {
        var results = _events
            .Where(x => x.AggregateTypeId == aggregateTypeId && x.AggregateId == aggregateId && x.Sequence > minSequence && (maxSequence is null ||  x.Sequence <= maxSequence))
            .Select(ToAggregateEvent);
        return await Task.FromResult(results);
    }
    
    private AggregateEvent ToAggregateEvent(EventDto x) 
    {
        var agentDto = _agents[x.AgentId];
        var agent = AgentDtoToAgent(agentDto!);
        var aggregateEvent = new AggregateEvent(_aggregateTypes[x.AggregateTypeId]!, x.AggregateId, _eventTypes[x.EventTypeId]!, x.Sequence, x.Data, agent, x.EventTime);
        return aggregateEvent;
    }

    public async Task SaveSnapshot(SnapshotDto snapshotDto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _snapshotDictionary[(snapshotDto.AggregateTypeId, snapshotDto.AggregateId)] = snapshotDto;
    }

    public async Task<IOption<Snapshot>> GetSnapshot(int aggregateTypeId, long aggregateId, int version,
        CancellationToken cancellationToken, long? maxSequence = null)
    {
        await Task.CompletedTask;
        _snapshotDictionary.TryGetValue((aggregateTypeId, aggregateId), out var dto);
        
        if (dto is null)
        {
            return new None<Snapshot>();
        }
        
        if (maxSequence is not null && dto.Sequence > maxSequence)
        {
            return new None<Snapshot>();
        }
        
        if (dto.Version != version)
        {
            return new None<Snapshot>();
        }
        
        return new Some<Snapshot>(new Snapshot(_aggregateTypes[dto.AggregateTypeId]!, dto.AggregateId, version, dto.Sequence,
                dto.State));
    }

    public IEnumerable<AggregateEvent> GetCapturedEvents()
    {
        var results = _events.Select(x =>
        {
            var agentDto = _agents[x.AgentId];
            var agent = AgentDtoToAgent(agentDto!);
            var aggregateEvent = new AggregateEvent(_aggregateTypes[x.AggregateTypeId]!, x.AggregateId, _eventTypes[x.EventTypeId]!, x.Sequence, x.Data, agent, x.EventTime);
            return aggregateEvent;
        });
        return results;
    }

    private Agent AgentDtoToAgent(AgentDto agentDto)
    {
        return new Agent(_agentTypes[agentDto.AgentTypeId]!, agentDto.SystemId, agentDto.AgentKey);
    }
}