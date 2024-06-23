using Evercore.Context;

namespace Evercore;

public interface IEventStore
{
    /// <summary>
    ///  Executes a task within a context.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WithContext(Func<IEventStoreContext, CancellationToken, Task> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a task within a context that returns a value. 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> WithContext<T>(Func<IEventStoreContext, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a task within a read-only context.
    /// </summary>
    /// <remarks>
    /// <para>Readonly context - events cannot be applied but can be used to hydrate existing aggregates.</para>
    /// </remarks>
    /// <param name="action">The task to be executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WithReadonlyContext(Func<IEventStoreReadContext, CancellationToken, Task> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a task within a read-only context.
    /// </summary>
    /// <remarks>
    /// <para>This is for readonly contexts where no events can be applied/stored and returns a value.</para>
    /// </remarks>
    /// <param name="action">The task to be executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<T> WithReadonlyContext<T>(Func<IEventStoreReadContext, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default);
}