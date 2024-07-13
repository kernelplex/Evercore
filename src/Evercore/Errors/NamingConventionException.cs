using Evercore.Errors;

namespace Evercore.Exceptions;

public class NamingConventionException : EventStoreException
{
    public Type? Type { get; }
    public string Name { get; }

    public NamingConventionException(string message, Type type, string name)
    {
        Type = type;
        Name = name;
    }
    public NamingConventionException(string message, string name)
    {
        Name = name;
    }
}