using Evercore.StrongTypes;

namespace Evercore.Data;

/// <summary>
/// Represents an event in the system.
/// </summary>
public interface IEvent
{
    public static abstract EventType EventType { get; }
}