using Evercore.Data;
using KernelPlex.Tools.Monads.Options;

namespace Evercore.Context;
public interface IEventStoreReadContext
{
    /// <summary>
    /// Loads an aggregate from the event store.
    /// </summary>
    /// <typeparam name="T">The type of aggregate to load. Must implement <see cref="IAggregate"/>.</typeparam>
    /// <param name="initializer">The initializer function used to create a new instance of the aggregate.</param>
    /// <param name="id">The identifier of the aggregate to load.</param>
    /// <param name="maxSequence">The maximum sequence number of events to load. Defaults to null, which loads all events.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An <see cref="IOption{T}"/> containing the loaded aggregate if it exists, or <see cref="None{T}"/> otherwise.
    /// </returns>
    Task<IOption<T>> Load<T>(Func<long, T> initializer, long id, long? maxSequence = null,
        CancellationToken cancellationToken = default) where T : IAggregate;
}