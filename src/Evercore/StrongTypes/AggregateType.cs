namespace Evercore.StrongTypes;

/// <summary>
/// Represents an aggregate type.
/// </summary>
public record AggregateType : StrongName
{
    public AggregateType(string value) : base(value)
    {
    }

    public static explicit operator AggregateType(string value) => new(value);
    public static implicit operator AggregateType(Enum value) => new(value.ToString());
}