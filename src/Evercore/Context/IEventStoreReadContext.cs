using Evercore.Data;
using KernelPlex.Tools.Monads.Options;

namespace Evercore.Context;
public interface IEventStoreReadContext
{
    /// <summary>
    /// Loads an aggregate of type T from the event store based on the specified ID.
    /// </summary>
    /// <param name="id">The ID of the aggregate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="T">The type of the aggregate.</typeparam>
    /// <returns>An optional aggregate of type T.</returns>
    Task<IOption<TAggregate>> Load<TAggregate>(long id, long? maxSequence = null, CancellationToken cancellationToken = default)
        where TAggregate : IAggregate;
}