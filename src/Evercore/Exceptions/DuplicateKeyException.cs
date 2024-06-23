namespace Evercore.Exceptions;

public class DuplicateKeyException : EventStoreException
{
    public int AggregateTypeId { get; }
    public string NaturalKey { get; }
    
    public DuplicateKeyException(string naturalKey, int aggregateTypeId)
    {
        NaturalKey = naturalKey;
        AggregateTypeId = aggregateTypeId;
    }

    public DuplicateKeyException(string? message, int aggregateTypeId, string naturalKey) : base(message)
    {
        NaturalKey = naturalKey;
        AggregateTypeId = aggregateTypeId;
    }

    public DuplicateKeyException(string? message, Exception? innerException, int aggregateTypeId, string naturalKey) : base(message, innerException)
    {
        NaturalKey = naturalKey;
        AggregateTypeId = aggregateTypeId;
    }

}