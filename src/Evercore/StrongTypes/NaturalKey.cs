namespace Evercore.StrongTypes;

/// <summary>
/// Represents a natural key for an entity.
/// </summary>
public record NaturalKey : StrongName
{
    public NaturalKey(string value) : base(value)
    {
    }

    public static explicit operator NaturalKey(string value) => new(value);
    public static explicit operator NaturalKey(Guid value) => new(value.ToString());
}