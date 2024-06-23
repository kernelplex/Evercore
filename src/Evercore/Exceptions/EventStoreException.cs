namespace Evercore.Exceptions;

public abstract class EventStoreException : Exception
{
    protected EventStoreException() : base()
    {
    }

    protected EventStoreException(string? message) : base(message)
    {
    }

    protected EventStoreException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}