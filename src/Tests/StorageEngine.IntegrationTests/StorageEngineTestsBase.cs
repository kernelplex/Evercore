using Evercore.Data;
using Evercore.Exceptions;
using Evercore.Monads;
using Evercore.Storage;
using Evercore.StrongTypes;
using FluentAssertions;

namespace StorageEngine.IntegrationTests;

public abstract class StorageEngineTestsBase
{
    protected abstract IStorageEngine? StorageEngine { get; set; }

    [Fact]
    public async Task GetAggregateTypeId_ShouldRetrieveANewId()
    {
        var aggregateName = (AggregateType) "TestGetAggregateId";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(
            aggregateName, cancellationToken: CancellationToken.None);

        aggregateTypeId.Should().BePositive();
    }
    
    [Fact]
    public async Task GetAggregateTypeId_ShouldRetrieveExistingId()
    {
        
        var aggregateName = (AggregateType) "TestGetAggregateId2";
        
        // First call.
        var aggregateTypeIdFirst = await StorageEngine!.GetAggregateTypeId(
            aggregateName, cancellationToken: CancellationToken.None);
        
        // Second call.
        var aggregateTypeIdSecond = await StorageEngine!.GetAggregateTypeId(
            aggregateName, cancellationToken: CancellationToken.None);

        aggregateTypeIdSecond.Should().Be(aggregateTypeIdFirst);
    }
    
    
    [Fact]
    public async Task GetAggregateTypeId_ShouldRetrieveDifferentIdsForDifferentAggregates()
    {
        
        var aggregateName1 = (AggregateType) "TestGetAggregateId_unique1";
        var aggregateName2 = (AggregateType) "TestGetAggregateId_unique2";
        
        // First call.
        var aggregateTypeIdFirst = await StorageEngine!.GetAggregateTypeId(
            aggregateName1, cancellationToken: CancellationToken.None);
        
        // Second call.
        var aggregateTypeIdSecond = await StorageEngine!.GetAggregateTypeId(
            aggregateName2, cancellationToken: CancellationToken.None);

        aggregateTypeIdSecond.Should().NotBe(aggregateTypeIdFirst);
    }

    [Fact]
    public async Task CreateAggregate_ShouldGenerateId()
    {
        var aggregateType = (AggregateType) "User";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);
        var naturalKey = (NaturalKey) "chavez01@example.com";

        var id = await StorageEngine!.CreateAggregate(aggregateTypeId, naturalKey, CancellationToken.None);

        id.Should().BePositive("CreateAggregate should always generate a positive value.");
    }

    [Fact]
    public async Task CreateAggregate_WithoutNaturalKey_ShouldGenerateId()
    {
        var aggregateType = (AggregateType) "User";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);

        var id = await StorageEngine!.CreateAggregate(aggregateTypeId, null, cancellationToken: CancellationToken.None);

        id.Should().BePositive("CreateAggregate should always generate a positive value.");
        
    }
    
    [Fact]
    public async Task CreateAggregate_WithoutNaturalKeyMultipleTimes_ShouldGenerateIds()
    {
        var aggregateType = (AggregateType) "User";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);

        var id = await StorageEngine!.CreateAggregate(aggregateTypeId, null, cancellationToken: CancellationToken.None);
        var id2 = await StorageEngine!.CreateAggregate(aggregateTypeId, null, cancellationToken: CancellationToken.None);

        id.Should().BePositive("CreateAggregate should always generate a positive value.");
        id2.Should().BePositive("CreateAggregate should always generate a positive value.");
    }
    
    [Fact]
    public async Task CreateAggregate_DuplicateNaturalKeysShouldFail()
    {
        var aggregateType = (AggregateType) "User";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);
        var naturalKey = (NaturalKey) "chavezz01@example.com";

        var id = await StorageEngine!.CreateAggregate(aggregateTypeId, naturalKey, CancellationToken.None);
        var action = async () =>
        {
            _ = await StorageEngine!.CreateAggregate(aggregateTypeId, naturalKey, CancellationToken.None);
        };
        await action.Should().ThrowAsync<DuplicateKeyException>(
                "Should throw DuplicateKeyException on duplicate natural keys on the same aggregate.")
            .Where(x => 
                x.AggregateTypeId == aggregateTypeId && x.NaturalKey == naturalKey);
    }

    [Fact]
    public async Task GetEventTypeId_ShouldGenerateIdForNewEventType()
    {
        var eventType = (EventType) "MyNewEventType";
        var id = await StorageEngine!.GetEventTypeId(eventType, CancellationToken.None);

        id.Should().BePositive();
    }
    
    
    [Fact]
    public async Task GetEventTypeId_MultipleTimesWithTheSameName_ShouldReturnSameId()
    {
        var eventType = (EventType) "MyNewEventType";
        var id = await StorageEngine!.GetEventTypeId(eventType, CancellationToken.None);
        var id2 = await StorageEngine!.GetEventTypeId(eventType, CancellationToken.None);

        id.Should().BePositive();
        id2.Should().Be(id);
    }

    [Fact]
    public async Task GetAgentTypeId_ShouldReturnPositive()
    {
        var agentType = (AgentType) "UserService";
        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);
        agentTypeId.Should().BePositive();
    }
    
    [Fact]
    public async Task GetAgentTypeId_MultipleCallsWithTheSameName_ShouldReturnSameId()
    {
        var agentType = (AgentType) "UserService";
        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);
        var agentTypeId2 = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);
        agentTypeId.Should().BePositive();
        agentTypeId2.Should().Be(agentTypeId);
    }

    [Fact]
    public async Task GetAgentId_ShouldReturnPositiveId()
    {
        var agentType = (AgentType) "Service";

        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);
        long systemId = 14;
        var agentKey = (AgentKey) "UserService";
        var id = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        id.Should().BePositive();
    }
    
    [Fact]
    public async Task GetAgentId_MultipleCallsForSameName_ShouldReturnSameId()
    {
        var agentType = (AgentType) "Service";

        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);
        long systemId = 12;
        var agentKey = (AgentKey) "UserService";
        
        var id = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        var id2 = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        
        id.Should().BePositive();
        id2.Should().Be(id);
    }
    
    
    [Fact]
    public async Task GetAgentId_MultipleCallsWithNullKey_ReturnSameId()
    {
        var agentType = (AgentType) "Service";

        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);
        long systemId = 12;
        AgentKey? agentKey = null;
        
        var id = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        var id2 = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        
        id.Should().BePositive();
        id2.Should().Be(id);
    }
    
    [Fact]
    public async Task GetAgentId_MultipleCallsWithNullSystemId_ShouldReturnSameId()
    {
        var agentType = (AgentType) "Service";

        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);
        long? systemId = null;
        var agentKey = (AgentKey) "MahAgent";
        
        var id = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        var id2 = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        
        id.Should().BePositive();
        id2.Should().Be(id);
    }
    
    [Fact]
    public async Task GetAgentId_MultipleCallsWithNullSystemIdAndNullKey_ShouldReturnSameId()
    {
        var agentType = (AgentType) "Service";

        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);
        long? systemId = null;
        AgentKey? agentKey = null;
        
        var id = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        var id2 = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);
        
        id.Should().BePositive();
        id2.Should().Be(id);
    }
   
    [Fact]
    public async Task StorageEngine_ShouldBeAbleToStoreAnd_RetrieveEvents()
    {
        var aggregateType = (AggregateType) "SampleAggregate";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);
        
        var createdEventType = (EventType) "Created";
        var updatedEventType = (EventType) "Updated";
        EventType[] eventTypes = [createdEventType, updatedEventType];
        var createdEventTypeId = await StorageEngine!.GetEventTypeId(createdEventType, CancellationToken.None);
        var updatedEventTypeId = await StorageEngine!.GetEventTypeId(updatedEventType, CancellationToken.None);
        
        var agentType = (AgentType) "Service";
        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);

        var aggregateId = await StorageEngine!.CreateAggregate(aggregateTypeId, null, CancellationToken.None);
        
        long? systemId = null;
        AgentKey? agentKey = null;
        var agentId = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);

        var agent = new Agent( agentType, systemId, agentKey);

        var events = new [] {
            new EventDto(aggregateTypeId, aggregateId, createdEventTypeId, 1, "", agentId,
                DateTime.Parse("2024-05-15T00:00:01Z").ToUniversalTime()),
            new EventDto(aggregateTypeId, aggregateId, updatedEventTypeId, 2, "", agentId,
                DateTime.Parse("2024-05-15T00:00:01Z").ToUniversalTime())
        };


        await StorageEngine!.StoreEvents(events, CancellationToken.None);
        var retrievedEvents =
            (await StorageEngine!.GetAggregateEvents(aggregateTypeId, aggregateId, 0, CancellationToken.None))
            .ToArray();

        retrievedEvents.Count().Should().Be(2);
        for(var x = 0; x < events.Length; ++x)
        {
            retrievedEvents[x].EventType.Should().Be(eventTypes[x]);
            retrievedEvents[x].AggregateType.Should().Be(aggregateType);
            retrievedEvents[x].AggregateId.Should().Be(aggregateId);
            retrievedEvents[x].Data.Should().Be(events[x].Data);
            retrievedEvents[x].Sequence.Should().Be(events[x].Sequence);
            retrievedEvents[x].Agent.Should().BeEquivalentTo(agent);
            retrievedEvents[x].EventTime.Should().Be(events[x].EventTime);
            
        }
    }

    [Fact]
    public async Task StoreEvents_WhenDuplicateSequencesForAnAggregate_ShouldFail()
    {
        var aggregateType = (AggregateType) "SampleAggregate";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);
        
        var createdEventType = (EventType) "Created";
        var updatedEventType = (EventType) "Updated";
        var createdEventTypeId = await StorageEngine!.GetEventTypeId(createdEventType, CancellationToken.None);
        var updatedEventTypeId = await StorageEngine!.GetEventTypeId(updatedEventType, CancellationToken.None);
        
        var agentType = (AgentType) "Service";
        var agentTypeId = await StorageEngine!.GetAgentTypeId(agentType, CancellationToken.None);

        var aggregateId = await StorageEngine!.CreateAggregate(aggregateTypeId, null, CancellationToken.None);
        
        long? systemId = null;
        AgentKey? agentKey = null;
        var agentId = await StorageEngine!.GetAgentId(agentTypeId, agentKey, systemId, CancellationToken.None);

        var events = new [] {
            new EventDto(aggregateTypeId, aggregateId, createdEventTypeId, 1, "", agentId,
                DateTime.Parse("2024-05-15T00:00:01Z")),
            new EventDto(aggregateTypeId, aggregateId, updatedEventTypeId, 1, "", agentId,
                DateTime.Parse("2024-05-15T00:00:01Z"))
        };

        var action = async () =>
        {
            await StorageEngine!.StoreEvents(events, CancellationToken.None);
        };
        await action.Should().ThrowAsync<AggregateSequenceException>();
    }
    
    [Fact]
    public async Task StorageEngine_ShouldBeAbleToStoreAndRetrieveSnapshots()
    {
        var aggregateType = (AggregateType) "SampleAggregate";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);
        var aggregateId = await StorageEngine!.CreateAggregate(aggregateTypeId, null, CancellationToken.None);
        var snapshot = new SnapshotDto(aggregateTypeId, aggregateId, 1, 12, "");
        await StorageEngine!.SaveSnapshot(snapshot, CancellationToken.None);

        var maybeRetrievedSnapshot = await StorageEngine!.GetSnapshot(aggregateTypeId, aggregateId, 1, CancellationToken.None);

        maybeRetrievedSnapshot.Should().BeOfType(typeof(Some<Snapshot>));
        var retrievedSnapshot = ((Some<Snapshot>) maybeRetrievedSnapshot).Value;
        
        retrievedSnapshot.AggregateType.Should().Be(aggregateType);
        retrievedSnapshot.AggregateId.Should().Be(aggregateId);
        retrievedSnapshot.SnapshotVersion.Should().Be(snapshot.Version);
        retrievedSnapshot.Sequence.Should().Be(snapshot.Sequence);
        retrievedSnapshot.State.Should().Be(snapshot.State);
    }
    
    [Fact]
    public async Task GetSnapshot_ShouldNotRetrieveOutdatedSnapshots()
    {
        var aggregateType = (AggregateType) "SampleAggregate";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);
        var aggregateId = await StorageEngine!.CreateAggregate(aggregateTypeId, null, CancellationToken.None);
        var snapshot = new SnapshotDto(aggregateTypeId, aggregateId, 1, 12, "");
        await StorageEngine!.SaveSnapshot(snapshot, CancellationToken.None);

        var retrievedSnapshot = await StorageEngine!.GetSnapshot(aggregateTypeId, aggregateId, 2, CancellationToken.None);
        retrievedSnapshot.Should().BeOfType<None<Snapshot>>();
    }
    
    [Fact]
    public async Task GetSnapshot_ShouldReturnNoneIfNoSnapshotExists()
    {
        var aggregateType = (AggregateType) "SampleAggregate";
        var aggregateTypeId = await StorageEngine!.GetAggregateTypeId(aggregateType, CancellationToken.None);
        var aggregateId = await StorageEngine!.CreateAggregate(aggregateTypeId, null, CancellationToken.None);
        var retrievedSnapshot = await StorageEngine!.GetSnapshot(aggregateTypeId, aggregateId, 1, CancellationToken.None);
        retrievedSnapshot.Should().BeOfType<None<Snapshot>>();
    }
    
    
}