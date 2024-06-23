using Evercore.StrongTypes;

namespace Evercore.Data;

/// <summary>
/// Represents an agent use for tracing who or what process is tied to events.
/// </summary>
public record Agent
{
    public AgentType AgentType { get; }
    public long? SystemId { get; }
    public AgentKey? AgentKey { get; }

    public override string ToString()
    {
        return AgentType + (SystemId is not null ? $":{SystemId}" : "") + (AgentKey is not null ? $":{AgentKey}" : "");
    }

    public Agent(AgentType agentType, long systemId)
    {
        AgentType = agentType;
        SystemId = systemId;
    }
    
    public Agent(AgentType agentType, AgentKey agentKey)
    {
        AgentType = agentType;
        AgentKey = agentKey;
    }
    
    public Agent(AgentType agentType, long? systemId, AgentKey? agentKey)
    {
        AgentType = agentType;
        AgentKey = agentKey;
        SystemId = systemId;
    }
}