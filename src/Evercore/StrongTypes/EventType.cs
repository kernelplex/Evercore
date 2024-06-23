namespace Evercore.StrongTypes;

/// <summary>
/// Represents an event type.
/// </summary>
public record EventType : StrongName
{
    public EventType(string value) : base(value)
    {
    }

    public static explicit operator EventType(string value) => new(value);
    public static implicit operator EventType(Enum value) => new(value.ToString());
}