namespace Evercore.Context;

/// <summary>
/// Represents the context for interacting with an event store. Inherits from IEventStoreReadContext and IEventStoreWriteContext.
/// </summary>
public interface IEventStoreContext : IEventStoreReadContext, IEventStoreWriteContext
{
}