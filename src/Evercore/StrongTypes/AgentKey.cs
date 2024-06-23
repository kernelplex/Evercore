namespace Evercore.StrongTypes;

/// <summary>
/// Represents a strong key for an agent.
/// </summary>
public record AgentKey : StrongName
{
    public AgentKey(string value) : base(value)
    {
    }

    public static explicit operator AgentKey(string value) => new(value);
}