using Evercore.StrongTypes;

namespace Evercore.Data;

/// <summary>
/// Represents an aggregate event.
/// </summary>
public record AggregateEvent(
    AggregateType AggregateType,
    long AggregateId,
    EventType EventType,
    long Sequence,
    string Data,
    Agent Agent,
    DateTime EventTime);