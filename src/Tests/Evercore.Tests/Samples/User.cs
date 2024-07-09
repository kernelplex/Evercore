using System.Text.Json;
using Evercore.Data;

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
public class User: ISnapshotAggregate
{
    [System.Text.Json.Serialization.JsonIgnore]
    public long Id { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public long Sequence { get; private set; }
    public string FirstName { get; private set; } = "";
    public string LastName { get; private set; } = "";
    public string Email { get; private set; } = "";
    
    public static AggregateType AggregateType => (AggregateType) nameof(AggregateTypes.User);
    public static int SnapshotVersion => 1;
   
    public static IAggregate Initialize(long id)
    {
        return new User()
        {
            Id = id
        };
    }

    public void ApplyEvent(IEvent @event, long sequence, Agent agent, DateTime eventTime)
    {
        switch (@event)
        {
            case UserCreatedEvent userCreated:
                FirstName = userCreated.FirstName;
                LastName = userCreated.LastName;
                Email = userCreated.Email;
                break;
            case UserUpdatedEvent userUpdatedEvent:
                FirstName = userUpdatedEvent.FirstName ?? FirstName;
                LastName = userUpdatedEvent.LastName ?? LastName;
                Email = userUpdatedEvent.Email ?? Email;
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
        FirstName = state.FirstName;
        LastName = state.LastName;
        Email = state.Email;
        Sequence = snapshot.Sequence;
    }

    public Snapshot TakeSnapshot()
    {
        var state = new UserState(FirstName, LastName, Email);
        var serializedState = JsonSerializer.Serialize(state);
        return  new Snapshot(AggregateType, Id, SnapshotVersion, Sequence, serializedState);
    }
}