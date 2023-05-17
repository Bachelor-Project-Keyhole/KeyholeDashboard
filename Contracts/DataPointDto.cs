namespace Contracts;

public record DataPointDto(
    string Id,
    string OrganizationId,
    string DataPointKey, 
    string DisplayName,
    FormulaDto Formula,
    bool DirectionIsUp,
    bool ComparisonIsAbsolute);