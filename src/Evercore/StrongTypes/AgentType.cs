namespace Evercore.StrongTypes;

/// <summary>
/// Represents an agent type.
/// </summary>
public record AgentType : StrongName
{
    public AgentType(string value) : base(value)
    {
    }
    
    public static explicit operator AgentType(string value) => new(value);
    public static implicit operator AgentType(Enum value) => new(value.ToString());
}