namespace Evercore.Data;

/// <summary>
/// The Snapshot class represents a snapshot of an aggregate in the storage engine.
/// </summary>
/// <remarks>
/// A snapshot contains information about the aggregate type, aggregate ID, snapshot version, sequence number, and
/// the state of the aggregate.
/// </remarks>
public record Snapshot
{
    /// <summary>
    /// The AggregateType property represents the type of the aggregate.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> indicating the type of the aggregate.
    /// </value>
    public AggregateType AggregateType { get; }

    /// <summary>
    /// Property AggregateId.
    /// </summary>
    /// <remarks>
    /// <para>This property represents the unique identifier of an aggregate. It is used to identify and locate the aggregate in the storage engine.</para>
    /// </remarks>
    /// <value>
    /// A <see cref="long"/> value representing the unique identifier of the aggregate.
    /// </value>
    public long AggregateId { get; }

    /// <summary>
    /// Snapshot version property represents the version of this snapshot.
    /// </summary>
    /// <remarks>
    /// This property is used to track the snapshot version which can be applied to the aggregate
    /// (newer versions deprecate older snapshots).
    /// </remarks>
    /// <value>
    /// An <see cref="int"/> value representing the version of the snapshot.
    /// </value>
    public int SnapshotVersion { get; }

    /// <summary>
    /// The Sequence property represents the sequence number of a snapshot.
    /// </summary>
    /// <value>
    /// A <see cref="long"/> value representing the sequence number of the snapshot.
    /// </value>
    public long Sequence { get; }

    /// <summary>
    /// The State property represents the state of an aggregate in the storage engine.
    /// </summary>
    /// <remarks>
    /// A state is a serialized representation of the data contained within an aggregate.
    /// </remarks>
    /// <value>
    /// A <see cref="string"/> value representing the serialized state of the aggregate.
    /// </value>
    public string State { get; }

    /// <summary>
    /// The Snapshot class represents a snapshot of an aggregate in the storage engine.
    /// </summary>
    /// <remarks>
    /// A snapshot contains information about the aggregate type, aggregate ID, snapshot version, sequence number, and
    /// the state of the aggregate.
    /// </remarks>
    public Snapshot(AggregateType aggregateType, long aggregateId, int snapshotVersion, long sequence, string state)
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        SnapshotVersion = snapshotVersion;
        Sequence = sequence;
        State = state;
    }

    /// <summary>
    /// The Snapshot class represents a snapshot of an aggregate in the storage engine.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A snapshot contains information about the aggregate type, aggregate ID, snapshot version, sequence number, and
    /// the state of the aggregate.
    /// </para>
    /// <para>
    /// This version is to support SQLite since it always represents INTEGER types as int64 even though in the case of
    /// SnapShotVersion, it ought to always be in bounds of int32.
    /// </para>
    /// </remarks>
    public Snapshot(AggregateType aggregateType, long aggregateId, long snapshotVersion, long sequence, string state)
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        SnapshotVersion = (int) snapshotVersion;
        Sequence = sequence;
        State = state;
    }
}