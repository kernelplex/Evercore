using Evercore.StrongTypes;

namespace Evercore.Data;

/// <summary>
/// Represents an aggregate DTO.
/// </summary>
/// <param name="Id">The ID of the aggregate.</param>
/// <param name="AggregateTypeId">The ID of the aggregate type.</param>
/// <param name="NaturalKey">The natural key of the aggregate.</param>
public record AggregateDto(long Id, int AggregateTypeId, NaturalKey? NaturalKey);