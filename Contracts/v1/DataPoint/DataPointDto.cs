namespace Contracts.v1.DataPoint;

public record DataPointDto(
    string Id,
    string OrganizationId,
    string DataPointKey, 
    string DisplayName,
    FormulaDto Formula,
    double LatestValue,
    bool DirectionIsUp,
    bool ComparisonIsAbsolute);