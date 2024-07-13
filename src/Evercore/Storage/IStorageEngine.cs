using Evercore.Data;
using Evercore.Errors;
using Evercore.StrongTypes;
using KernelPlex.Tools.Monads.Options;
using KernelPlex.Tools.Monads.Results;

namespace Evercore.Storage;

/// <summary>
/// Represents a storage engine for persisting and retrieving data.
/// </summary>
public interface IStorageEngine
{
    /// <summary>
    /// Retrieves the aggregate type ID for the given <paramref name="aggregateType"/>.
    /// </summary>
    /// <param name="aggregateType">The aggregate type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the aggregate type ID.</returns>
    Task<int> GetAggregateTypeId(AggregateType aggregateType, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new aggregate with the specified aggregate type ID, natural key, and cancellation token.
    /// </summary>
    /// <param name="aggregateTypeId">The aggregate type ID.</param>
    /// <param name="naturalKey">The natural key of the aggregate. Can be null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns a task representing the asynchronous operation. The result contains an instance of <see cref="IResult{TSuccess, TError}"/> where TSuccess is a long representing the newly generated aggregate ID, and TError is <see cref="DuplicateKeyError"/> if the natural key is duplicate.</returns>
    public Task<IResult<long, DuplicateKeyError>> CreateAggregate(int aggregateTypeId, NaturalKey? naturalKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the event type ID for the given <paramref name="eventType"/>.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the event type ID.</returns>
    Task<int> GetEventTypeId(EventType eventType, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the agent ID for the given parameters.
    /// </summary>
    /// <param name="agentTypeId">The ID of the agent type.</param>
    /// <param name="agentKey">The agent key.</param>
    /// <param name="systemId">The ID of the system.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the agent ID.</returns>
    Task<long> GetAgentId(int agentTypeId, AgentKey? agentKey, long? systemId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the agent type ID for the given <paramref name="agentType"/>.
    /// </summary>
    /// <param name="agentType">The agent type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the agent type ID.</returns>
    Task<int> GetAgentTypeId(AgentType agentType, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the given event data in the storage engine.
    /// </summary>
    /// <param name="eventDtos">The event data to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The result contains a boolean indicating whether the event data was successfully stored and any sequence errors encountered.</returns>
    Task<IResult<bool, SequenceError>>
        StoreEvents(IEnumerable<EventDto> eventDtos, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the aggregate events for the specified <paramref name="aggregateTypeId"/>, <paramref name="id"/>, and <paramref name="minSequence"/>.
    /// </summary>
    /// <param name="aggregateTypeId">The aggregate type ID.</param>
    /// <param name="id">The aggregate ID.</param>
    /// <param name="minSequence">The event sequence number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="maxSequence">If not null, specifies the maximum sequence to return.</param>
    /// <returns>A task representing the asynchronous operation. The result contains a collection of aggregate events.</returns>
    Task<IEnumerable<AggregateEvent>> GetAggregateEvents(int aggregateTypeId, long id, long minSequence,
        CancellationToken cancellationToken, long? maxSequence = null);

    /// <summary>
    /// Saves the snapshot specified by the <paramref name="snapshotDto"/>.
    /// </summary>
    /// <param name="snapshotDto">The snapshot to be saved.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveSnapshot(SnapshotDto snapshotDto, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the snapshot for the given aggregate type ID, aggregate ID, and version.
    /// </summary>
    /// <param name="aggregateTypeId">The ID of the aggregate type.</param>
    /// <param name="aggregateId">The ID of the aggregate.</param>
    /// <param name="version">The version of the snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="maxSequence">If not null, specifies the maximum snapshot sequence to return.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an optional snapshot.</returns>
    Task<IOption<Snapshot>> GetSnapshot(int aggregateTypeId, long aggregateId, int version,
        CancellationToken cancellationToken, long? maxSequence = null);
}