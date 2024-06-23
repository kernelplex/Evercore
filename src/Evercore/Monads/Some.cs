namespace Evercore.Monads;

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class Some<T>(T value) : Option<T>
{
    public T Value { get; } = value;
}