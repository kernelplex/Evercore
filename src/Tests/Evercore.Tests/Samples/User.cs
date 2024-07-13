using System.Text.Json;
using Evercore.Context;
using Evercore.Data;
using Evercore.StrongTypes;

namespace Evercore.Tests.Samples;

public static class AggregateTypes
{
    public static readonly AggregateType User = (AggregateType) "User";
}

public static class UserEvents
{
    public static readonly AggregateType UserRegistered = (AggregateType) "UserRegistered";
    public static readonly AggregateType UserUpdated = (AggregateType) "UserUpdated";
}

public record UserState(string FirstName, string LastName, string Email);

public class User: ISnapshotAggregate
{
    [System.Text.Json.Serialization.JsonIgnore]
    public long Id { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public long Sequence { get; private set; }
    
    public UserState State { get; private set; }
    
    public string FirstName => State.FirstName;
    public string LastName => State.LastName;
    public string Email => State.Email;
    
    public static AggregateType AggregateType => (AggregateType) nameof(AggregateTypes.User);
    public static int SnapshotVersion => 1;

    private User(long id)
    {
        Id = id;
        State = new UserState("", "", "");
    }
   
    public static User Initialize(long id)
    {
        return new User(id);
    }

    public Agent AsAgent()
    {
        return new Agent((AgentType) "User", this.Id);
    }

    public void RegisterUser(IEventStoreContext ctx, string firstName, string lastName, string email)
    {
        var ev = new UserRegistered(firstName, lastName, email);
        ctx.Apply(ev, this, AsAgent());
    }

    public void ApplyEvent(IEvent @event, long sequence, Agent agent, DateTime eventTime)
    {
        switch (@event)
        {
            case UserRegistered userCreated:
                State = State with
                {
                    FirstName = userCreated.FirstName,
                    LastName = userCreated.LastName,
                    Email = userCreated.Email
                };
                break;
            case UserUpdated userUpdatedEvent:
                State = State with
                {
                    FirstName = userUpdatedEvent.FirstName ?? FirstName,
                    LastName = userUpdatedEvent.LastName ?? LastName,
                    Email = userUpdatedEvent.Email ?? Email
                };
                break;
            default:
                throw new InvalidOperationException("Event cannot be handled by this aggregate.");
        }
        Sequence = sequence;
    }

    public int GetSnapshotFrequency() => 10;

    public int GetCurrentSnapshotVersion() => 1;

    public void ApplySnapshot(Snapshot snapshot)
    {
        var state = JsonSerializer.Deserialize<UserState>(snapshot.State);
        if (state is null)
        {
            // TODO: Better exception.
            throw new InvalidOperationException("Unable to deserialize snapshot.");
        }

        State = new UserState(state.FirstName, state.LastName, state.Email);
        Sequence = snapshot.Sequence;
    }

    public Snapshot TakeSnapshot()
    {
        var serializedState = JsonSerializer.Serialize(State);
        return  new Snapshot(AggregateType, Id, SnapshotVersion, Sequence, serializedState);
    }
}