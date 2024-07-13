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
    /// Creates a new aggregate of type T in the event store context.
    /// </summary>
    /// <typeparam name="T">The type of aggregate to create.</typeparam>
    /// <param name="initializer">A function that initializes the aggregate instance.</param>
    /// <param name="naturalKey">An optional natural key for the aggregate.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IResult instance
    /// that wraps the created aggregate or a DuplicateKeyError if the natural key is already in use.</returns>
    Task<IResult<T, DuplicateKeyError>> Create<T>(Func<long, T> initializer, NaturalKey? naturalKey = null,
        CancellationToken cancellationToken = default) where T : IAggregate;

    /// <summary>
    /// Retrieves the captured events from the event store context.
    /// </summary>
    /// <returns>A collection of captured events.</returns>
    IEnumerable<AggregateEvent> GetCapturedEvents();
}