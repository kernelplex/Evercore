using System.Text.Json.Serialization;

namespace Evercore.Data;

public interface IAggregateTraits 
{
    /// <summary>
    /// Property Id.
    /// </summary>
    /// <remarks>
    /// <para>This property represents the unique identifier of an aggregate. It is used to identify and locate the aggregate in the storage engine.</para>
    /// </remarks>
    /// <value>
    /// A <see cref="long"/> value representing the unique identifier of the aggregate.
    /// </value>
    [JsonIgnore]
    long Id { get; }

    /// <summary>
    /// Property Sequence.
    /// </summary>
    /// <remarks>
    /// <para>This property represents the sequence number of the last event in the aggregate's event stream.</para>
    /// </remarks>
    /// <value>
    /// A <see cref="long"/> value representing the last event sequence number of the aggregate.
    /// </value>
    [JsonIgnore]
    long Sequence { get; }
    /// <summary>
    /// Applies an event to the aggregate object.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    /// <param name="sequence">The sequence number of the event.</param>
    /// <param name="agent">The agent who triggered the event.</param>
    /// <param name="eventTime">The timestamp of the event.</param>
    void ApplyEvent(IEvent @event, long sequence, Agent agent, DateTime eventTime);
    
}

/// <summary>
/// Interface for an aggregate object.
/// </summary>
public interface IAggregate: IAggregateTraits
{

    /// <summary>
    /// Static abstract property that represents the type of the aggregate.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the type of the aggregate.
    /// </value>
    /// <remarks>
    /// <para>This property is used to identify the type of the aggregate. It should be unique for each type of aggregate.</para>
    /// </remarks>
    static abstract AggregateType AggregateType { get; }

    /// <summary>
    /// Initializes an instance of the aggregate object with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the aggregate.</param>
    /// <returns>The initialized aggregate object.</returns>
    static abstract IAggregate Initialize(long id);
}

public interface ISnapshotAggregate : IAggregate, ISnapshotAggregateTraits
{
}

public interface ISnapshotAggregateTraits: IAggregateTraits
{
   
    /// <summary>
    /// Get the frequency (in sequence increments) which we should capture snapshots.
    /// </summary>
    /// <returns></returns>
    public int GetSnapshotFrequency();

    /// <summary>
    /// Get the current snapshot version applicable to this aggregate.
    /// </summary>
    /// <returns></returns>
    public int GetCurrentSnapshotVersion();
    
    /// <summary>
    /// Applies a snapshot to the aggregate object.
    /// </summary>
    /// <remarks>
    /// <para>Snapshots are used to improve the performance of hydrating an aggregate from the
    /// event store.</para>
    /// </remarks>
    /// <param name="snapshot">The snapshot to apply.</param>
    public void ApplySnapshot(Snapshot snapshot);

    /// <summary>
    /// Takes a snapshot of the aggregate state.
    /// </summary>
    /// <returns>The serialized snapshot of the aggregate object.</returns>
    public Snapshot TakeSnapshot();
}
