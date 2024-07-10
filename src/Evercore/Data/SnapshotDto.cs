namespace Evercore.Data;

/// <summary>
/// The SnapshotDto class represents a snapshot of an aggregate in the storage engine.
/// </summary>
public record SnapshotDto(int AggregateTypeId, long AggregateId, int Version, long Sequence, string State);
