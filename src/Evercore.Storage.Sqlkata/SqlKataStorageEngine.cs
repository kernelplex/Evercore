using System.Transactions;
using Evercore.Data;
using Evercore.Exceptions;
using Evercore.Monads;
using Evercore.StrongTypes;
using SqlKata.Execution;
// ReSharper disable UnusedMember.Local

namespace Evercore.Storage.SqlKata;

using QueryFactoryProvider = Func<CancellationToken, Task<QueryFactory>>;

public class SqlKataStorageEngine : IStorageEngine
{
    #region Fields

    private readonly QueryFactoryProvider _queryFactoryProvider;

    #endregion


    #region Constructors/Destructors

    public SqlKataStorageEngine(QueryFactoryProvider queryFactoryProvider)
    {
        _queryFactoryProvider = queryFactoryProvider;
    }

    #endregion

    #region Methods

    public async Task<int> GetAggregateTypeId(AggregateType aggregateType, CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Try to get any existing values.
        var selectQuery = queryFactory.Query(Tables.AggregateTypes)
            .Where("Name", aggregateType.Value)
            .Select("Id");

        var result = await selectQuery.FirstOrDefaultAsync<int?>(cancellationToken: cancellationToken);
        if (result is not null)
        {
            return result.Value;
        }

        // None, found, insert one.
        var insertQuery = queryFactory.Query(Tables.AggregateTypes);

        var id = await insertQuery.InsertGetIdAsync<int>(new
        {
            Name = aggregateType.Value
        }, cancellationToken: cancellationToken);

        transactionScope.Complete();

        return id;
    }

    public async Task<long> CreateAggregate(int aggregateTypeId, NaturalKey? naturalKey,
        CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (naturalKey is not null)
        {
            var existingKeySearch = queryFactory.Query(Tables.Aggregates)
                .Select("Id")
                .Where("NaturalKey", naturalKey.Value);

            long? existing = await existingKeySearch.FirstOrDefaultAsync<long?>(cancellationToken: cancellationToken);
            if (existing is not null)
            {
                throw new DuplicateKeyException("An aggregate exists with this natural key.", aggregateTypeId,
                    naturalKey.Value);
            }
        }

        var query = queryFactory.Query(Tables.Aggregates);
        var id = await query.InsertGetIdAsync<long>(new
        {
            AggregateTypeId = aggregateTypeId,
            NaturalKey = naturalKey?.Value,
            Sequence = 0
        }, cancellationToken: cancellationToken);

        transactionScope.Complete();
        return id;
    }

    public async Task<int> GetEventTypeId(EventType eventType, CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Try to get any existing values.
        var selectQuery = queryFactory.Query(Tables.EventTypes)
            .Where("Name", eventType.Value)
            .Select("Id");

        var result = await selectQuery.FirstOrDefaultAsync<int?>(cancellationToken: cancellationToken);
        if (result is not null)
        {
            return result.Value;
        }

        // None, found, insert one.
        var insertQuery = queryFactory.Query(Tables.EventTypes);

        var id = await insertQuery.InsertGetIdAsync<int>(new
        {
            Name = eventType.Value
        }, cancellationToken: cancellationToken);

        transactionScope.Complete();

        return id;
    }

    public async Task<long> GetAgentId(int agentTypeId, AgentKey? agentKey, long? systemId,
        CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Try to get any existing values.
        var selectQuery = queryFactory.Query(Tables.Agents)
            .Where("AgentTypeId", agentTypeId)
            .Where("AgentKey", agentKey?.Value)
            .Where("SystemId", systemId)
            .Select("Id");

        var result = await selectQuery.FirstOrDefaultAsync<long?>(cancellationToken: cancellationToken);
        if (result is not null)
        {
            return result.Value;
        }

        // None, found, insert one.
        var insertQuery = queryFactory.Query(Tables.Agents);

        var id = await insertQuery.InsertGetIdAsync<int>(new
        {
            AgentTypeId = agentTypeId,
            AgentKey = agentKey?.Value,
            SystemId = systemId
        }, cancellationToken: cancellationToken);

        transactionScope.Complete();

        return id;

    }

    public async Task<int> GetAgentTypeId(AgentType agentType, CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Try to get any existing values.
        var selectQuery = queryFactory.Query(Tables.AgentTypes)
            .Where("Name", agentType.Value)
            .Select("Id");

        var result = await selectQuery.FirstOrDefaultAsync<int?>(cancellationToken: cancellationToken);
        if (result is not null)
        {
            return result.Value;
        }

        // None, found, insert one.
        var insertQuery = queryFactory.Query(Tables.AgentTypes);

        var id = await insertQuery.InsertGetIdAsync<int>(new
        {
            Name = agentType.Value
        }, cancellationToken: cancellationToken);

        transactionScope.Complete();

        return id;
    }

    public async Task StoreEvents(IEnumerable<EventDto> eventDtos, CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var eventDtoList = eventDtos.ToList();
        var existingAggregateIds = eventDtoList.Select(
                x => (aggregateTypeId: x.AggregateTypeId, aggregateId: x.AggregateId))
            .Distinct();

        Dictionary<long, long> aggregateSequences = new Dictionary<long, long>();
        foreach (var (aggregateTypeId, aggregateId) in existingAggregateIds)
        {
            var sequence = await queryFactory.Query(Tables.Aggregates)
                .Select("Sequence")
                .Where("Id", aggregateId)
                .Where("AggregateTypeId", aggregateTypeId)
                .FirstOrDefaultAsync<long?>(cancellationToken: cancellationToken);
            aggregateSequences[aggregateId] = sequence!.Value;
        }

        foreach (var eventDto in eventDtoList)
        {
            if (eventDto.Sequence != ++aggregateSequences[eventDto.AggregateId])
            {
                throw new AggregateSequenceException("Aggregate sequence integrity error.", eventDto.AggregateTypeId,
                    @eventDto.AggregateId, @eventDto.Sequence);
            }

            // Ensure times are stored as universal times
            var eventTime = eventDto.EventTime.Kind != DateTimeKind.Utc
                ? eventDto.EventTime.ToUniversalTime()
                : eventDto.EventTime;

            await queryFactory.Query(Tables.AggregateEvents)
                .InsertAsync(new
                {
                    eventDto.AggregateTypeId,
                    eventDto.AggregateId,
                    eventDto.EventTypeId,
                    eventDto.Sequence,
                    eventDto.Data,
                    eventDto.AgentId,
                    EventTime = eventTime
                }, cancellationToken: cancellationToken);
        }

        // Update aggregate sequences
        foreach (var (aggregateId, newSequence) in aggregateSequences)
        {
            await queryFactory.Query(Tables.Aggregates)
                .Where("Id", aggregateId)
                .UpdateAsync(new
                {
                    Sequence = newSequence
                }, cancellationToken: cancellationToken);
        }

        transactionScope.Complete();
    }



    public async Task<IEnumerable<AggregateEvent>> GetAggregateEvents(int aggregateTypeId, long id, long sequence,
        CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);
        var eventQuery = queryFactory.Query($"{Tables.AggregateEvents} as ev")
            .Select("ev.AggregateId")
            .Select("aggT.Name as AggregateType")
            .Select("evT.Name as EventType")
            .Select("ev.Sequence")
            .Select("ev.Data")
            .Select("agtT.Name as AgentType")
            .Select("agt.AgentKey as AgentKey")
            .Select("agt.SystemId as SystemId")
            .Select("ev.EventTime")
            .LeftJoin($"{Tables.AggregateTypes} as aggT", "aggT.Id", "ev.AggregateTypeId")
            .LeftJoin($"{Tables.EventTypes} as evT", "evT.Id", "ev.EventTypeId")
            .LeftJoin($"{Tables.Agents} as agt", "agt.Id", "ev.AgentId")
            .LeftJoin($"{Tables.AgentTypes} as agtT", "agt.AgentTypeId", "agtT.Id")
            .Where("ev.AggregateTypeId", aggregateTypeId)
            .Where("ev.AggregateId", id)
            .Where("ev.Sequence", ">", sequence)
            .OrderBy("ev.Sequence");

        var results = await eventQuery.GetAsync<EventQueryResult>(cancellationToken: cancellationToken);
        List<AggregateEvent> events = new();
        foreach (var result in results)
        {
            events.Add((AggregateEvent) result);
        }

        return events;
    }


    public async Task SaveSnapshot(SnapshotDto snapshotDto, CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);
        var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var id = await queryFactory.Query(Tables.Snapshots)
            .InsertGetIdAsync<long>(new
            {
                snapshotDto.AggregateTypeId,
                snapshotDto.AggregateId,
                snapshotDto.Version,
                snapshotDto.Sequence,
                snapshotDto.State
            }, cancellationToken: cancellationToken);

        await queryFactory.Query(Tables.Snapshots)
            .Where("AggregateTypeId", snapshotDto.AggregateTypeId)
            .Where("AggregateId", snapshotDto.AggregateId)
            .Where("Id", "<>", id)
            .DeleteAsync(cancellationToken: cancellationToken);
        scope.Complete();
    }


    public async Task<Option<Snapshot>> GetSnapshot(int aggregateTypeId, long aggregateId, int version,
        CancellationToken cancellationToken)
    {
        using var queryFactory = await _queryFactoryProvider(cancellationToken);
        var snapshotQuery = queryFactory.Query(Tables.Snapshots)
            .Select("aggT.Name as AggregateType")
            .Select($"{Tables.Snapshots}.AggregateId as AggregateId")
            .Select($"{Tables.Snapshots}.Version as SnapshotVersion")
            .Select($"{Tables.Snapshots}.Sequence as Sequence")
            .Select($"{Tables.Snapshots}.State as State")
            .Join($"{Tables.AggregateTypes} as aggT", "aggT.Id", $"{Tables.Snapshots}.AggregateTypeId")
            .Where("AggregateTypeId", aggregateTypeId)
            .Where("AggregateId", aggregateId)
            .Where("Version", "=", version)
            .OrderByDesc("Sequence");

        var snapshotDto =
            await snapshotQuery.FirstOrDefaultAsync<SnapshotQueryResult>(cancellationToken: cancellationToken);

        return snapshotDto is null ? new None<Snapshot>() : new Some<Snapshot>(snapshotDto);
    }

    #endregion

    #region Inner Classes

    private class EventQueryResult
    {
        public long AggregateId { get; }
        public string AggregateType { get; }
        public string EventType { get; }
        public long Sequence { get; }
        public string Data { get; }
        public string AgentType { get; }
        public string? AgentKey { get; }
        public long? SystemId { get; }
        public DateTime EventTime { get; }

        public EventQueryResult(long aggregateId, string aggregateType, string eventType, long sequence, string data,
            string agentType, string? agentKey, long? systemId, string eventTime)
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType;
            EventType = eventType;
            Sequence = sequence;
            Data = data;
            AgentType = agentType;
            AgentKey = agentKey;
            SystemId = systemId;
            EventTime = DateTime.Parse(eventTime);
        }

        public EventQueryResult(
            long aggregateId,
            string aggregateType,
            string eventType,
            long sequence,
            string data,
            string agentType,
            string? agentKey,
            long? systemId,
            DateTime eventTime)
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType;
            EventType = eventType;
            Sequence = sequence;
            Data = data;
            AgentType = agentType;
            AgentKey = agentKey;
            SystemId = systemId;
            EventTime = eventTime;
        }


        public static explicit operator AggregateEvent(EventQueryResult result)
        {
            var agentKey = result.AgentKey is null ? null : (AgentKey) result.AgentKey;
            var systemId = result.SystemId;
            var agent = new Agent((AgentType) result.AgentType, systemId, agentKey);

            var aggregateEvent = new AggregateEvent(
                (AggregateType) result.AggregateType, result.AggregateId, (EventType) result.EventType, result.Sequence,
                result.Data,
                agent, result.EventTime);
            return aggregateEvent;
        }
    }

    private record SnapshotQueryResult
    {

        public string AggregateType { get; init; }
        public long AggregateId { get; init; }
        public int SnapshotVersion { get; init; }
        public long Sequence { get; init; }
        public string State { get; init; }

        public SnapshotQueryResult(string aggregateType, long aggregateId, int snapshotVersion, long sequence,
            string state)
        {
            this.AggregateType = aggregateType;
            this.AggregateId = aggregateId;
            this.SnapshotVersion = snapshotVersion;
            this.Sequence = sequence;
            this.State = state;
        }

        public SnapshotQueryResult(string aggregateType, long aggregateId, long snapshotVersion, long sequence,
            string state)
        {
            this.AggregateType = aggregateType;
            this.AggregateId = aggregateId;
            this.SnapshotVersion = (int) snapshotVersion;
            this.Sequence = sequence;
            this.State = state;
        }

        public static implicit operator Snapshot(SnapshotQueryResult value)
        {
            return new Snapshot(
                (AggregateType) value.AggregateType,
                value.AggregateId,
                value.SnapshotVersion,
                value.Sequence,
                value.State);
        }
    }

    #endregion
}