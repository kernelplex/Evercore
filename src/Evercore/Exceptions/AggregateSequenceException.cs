namespace Evercore.Exceptions;

public class AggregateSequenceException : EventStoreException
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public int AggregateTypeId { get; }
    public long AggregateId { get; }
    public long Sequence { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    
    public AggregateSequenceException(int aggregateTypeId, long aggregateId, long sequence)
    {
        Sequence = sequence;
        AggregateTypeId = aggregateTypeId;
        AggregateId = aggregateId;
    }

    public AggregateSequenceException(string? message, int aggregateTypeId, long aggregateId, long sequence)
        : base(message)
    {
        Sequence = sequence;
        AggregateTypeId = aggregateTypeId;
        AggregateId = aggregateId;
    }

    public AggregateSequenceException(string? message, int aggregateTypeId, long aggregateId, long sequence,
        Exception innerException) : base(message, innerException)
    {
        Sequence = sequence;
        AggregateTypeId = aggregateTypeId;
        AggregateId = aggregateId;
    }
}