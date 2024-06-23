using Evercore.StrongTypes;

namespace Evercore.Data;

/// <summary>
/// Represents a data transfer object (DTO) for the Agent class.
/// </summary>
public record AgentDto(int AgentTypeId, long? SystemId, AgentKey? AgentKey);
