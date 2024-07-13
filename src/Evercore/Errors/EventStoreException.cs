namespace Evercore.Errors;

public abstract class EventStoreException : Exception
{
    protected EventStoreException()
    {
    }

    protected EventStoreException(string? message) : base(message)
    {
    }

    protected EventStoreException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}