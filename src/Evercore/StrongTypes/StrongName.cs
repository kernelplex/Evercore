using Evercore.Data;
using Evercore.Exceptions;

namespace Evercore.StrongTypes;

/// <summary>
/// Represents a strong name.
/// </summary>
public abstract record StrongName
{
    public string Value { get; }

    public StrongName(string value)
    {
        if (value is null)
        {
            throw new NamingConventionException("The value cannot be null.", nameof(value));
        }
        if (value.Length > Limitations.MaximumSystemNameLength)
        {
            throw new NamingConventionException("The value cannot be null.", this.GetType(), nameof(value));
        }
        
        Value = value;
    }
    public static implicit operator String(StrongName strongName) => strongName.Value;
}