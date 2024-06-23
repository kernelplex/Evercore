using Evercore.Data;
using Evercore.Monads;
using Evercore.StrongTypes;

namespace Evercore;

/// <summary>
/// Represents an event store context manager.
/// </summary>
public interface IEventStoreContextManager
{
    /// <summary>
    /// Creates a new aggregate in the event store.
    /// </summary>
    /// <param name="aggregateType">The type of the aggregate.</param>
    /// <param name="naturalKey">The natural key of the aggregate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created aggregate.</returns>
    Task<long> CreateAggregate(AggregateType aggregateType, NaturalKey? naturalKey,
        CancellationToken cancellationToken);


    /// <summary>
    /// Retrieves a collection of aggregate events from the event store.
    /// </summary>
    /// <param name="aggregateType">The type of the aggregate.</param>
    /// <param name="id">The ID of the aggregate.</param>
    /// <param name="sequence">The sequence number starting point.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of aggregate events.</returns>
    Task<IEnumerable<AggregateEvent>> GetEvents(AggregateType aggregateType, long id, long sequence,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deserializes the given data into an object of the specified <paramref name="eventType"/>.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="data">The data to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    object Deserialize(EventType eventType, string data);

    /// <summary>
    /// Retrieves a snapshot of an aggregate at a specific version.
    /// </summary>
    /// <param name="aggregateType">The type of the aggregate.</param>
    /// <param name="id">The ID of the aggregate.</param>
    /// <param name="version">The version of the snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The snapshot of the aggregate at the specified version.</returns>
    Task<Option<Snapshot>> GetSnapshot(AggregateType aggregateType, long id, int version,
        CancellationToken cancellationToken);
}