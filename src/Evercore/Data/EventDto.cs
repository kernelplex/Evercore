namespace Evercore.Data;

/// <summary>
/// Represents a data transfer object for an event.
/// </summary>
public record EventDto(
    int AggregateTypeId,
    long AggregateId,
    int EventTypeId,
    long Sequence,
    string Data,
    long AgentId,
    DateTime EventTime);