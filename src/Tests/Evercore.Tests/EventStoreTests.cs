using Evercore.Context;
using Evercore.Data;
using Evercore.Storage;
using Evercore.StrongTypes;
using Evercore.Tests.Samples;
using FluentAssertions;
using KernelPlex.Tools.Monads.Options;

namespace Evercore.Tests;

public class EventStoreTests
{
    private EventStore _eventStore;
    private TransientMemoryStorageEngine _transientMemoryStorageEngine;
    public EventStoreTests()
    {
        _transientMemoryStorageEngine = new TransientMemoryStorageEngine();
        _eventStore = new EventStore(_transientMemoryStorageEngine);
        _eventStore.RegisterEventsInAssembly(typeof(UserCreatedEvent).Assembly);
    }

    record UserScenarioResults(object[] PublishedEvents, EventStoreContext CapturedContext, Agent Agent, User user);
    private async Task<UserScenarioResults> UserScenario(int updateCount = 15)
    {
        var agent = new Agent((AgentType) "UnitTest", (AgentKey) nameof(EventsAreCapturedInContext));
        var userCreated = new UserCreatedEvent("Robert", "Chavez", "chavez@example.com");
        var events = new List<object>() {userCreated};
        var dateTime = DateTime.UtcNow;
        EventStoreContext? capturedContext = null; 
        var user = await _eventStore.WithContext<User>(async (context, cancellationToken) =>
        {
            capturedContext = (EventStoreContext) context;
            var user = await context.Create<User>(cancellationToken: cancellationToken);
            context.Apply(userCreated, user, agent, dateTime);
            for (var j = 0; j < updateCount; ++j)
            {
                var userUpdated = new UserUpdatedEvent(Email: $"rchavez{j}@example.com");
                context.Apply(userUpdated, user, agent, dateTime);
                events.Add(userUpdated);
            }

            return user;
        });
        if (capturedContext is null)
        {
            throw new InvalidOperationException("capturedContext is null.");
        }
        return new UserScenarioResults(events.ToArray(), capturedContext, agent, user);
    }
    
    [Fact]
    public async Task EventsAreCapturedInContext()
    {
        var result = await UserScenario();
        var capturedEvents = result.CapturedContext.GetCapturedEvents();
        var types = capturedEvents.Select(x => _eventStore.Deserialize(x.EventType, x.Data));
        types.Should().BeEquivalentTo(result.PublishedEvents);
    }
    
    [Fact]
    public async Task EventsAreWrittenToStorageEngine()
    {
        var result = await UserScenario();
        var appliedEvents = result.CapturedContext.GetCapturedEvents().ToList();
        var storedEvents = _transientMemoryStorageEngine.GetCapturedEvents().ToList();

        appliedEvents.Count.Should().Be(storedEvents.Count);
        appliedEvents.Should().BeEquivalentTo(storedEvents);
    }

    [Fact]
    public async Task SnapshotsTaken()
    {
        var result = await UserScenario(11);
        var user = result.user;
        var userState = user.TakeSnapshot();
        var aggregates = result.CapturedContext.GetAggregatesRequiringSnapshots();
        var aggregatesWithSnapshots = aggregates.ToList();

        aggregatesWithSnapshots.Count.Should().Be(1);
        aggregatesWithSnapshots.First().Should().BeOfType<User>();
        var maybeSnapshot = await _eventStore.GetSnapshot(User.AggregateType, user.Id, User.SnapshotVersion, CancellationToken.None);
        maybeSnapshot.Should().BeOfType<Some<Snapshot>>();
        var snapshot = ((Some<Snapshot>) maybeSnapshot).Value;
        snapshot.Should().BeEquivalentTo(userState);
    }

}