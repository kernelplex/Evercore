using Evercore.Data;
using Evercore.Exceptions;
using Evercore.StrongTypes;
using KernelPlex.Tools.Monads.Results;

namespace Evercore.Context;

/// <summary>
/// Interface for writing to an event store context.
/// </summary>
public interface IEventStoreWriteContext: IEventStoreContextPublisher
{
    /// <summary>
    /// Creates a new aggregate object of type TAggregate in the event store context.
    /// </summary>
    /// <typeparam name="TAggregate">The type of aggregate to create.</typeparam>
    /// <param name="naturalKey">The natural key for the aggregate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created aggregate object.</returns>
    Task<IResult<TAggregate, DuplicateKeyError>> Create<TAggregate>(NaturalKey? naturalKey = null, CancellationToken cancellationToken = default)
        where TAggregate : IAggregate;

    /// <summary>
    /// Retrieves the captured events from the event store context.
    /// </summary>
    /// <returns>A collection of captured events.</returns>
    IEnumerable<AggregateEvent> GetCapturedEvents();
}