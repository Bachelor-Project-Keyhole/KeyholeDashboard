namespace Contracts;

public record DataPointDto(
    string Id,
    string OrganizationId,
    string DataPointKey, 
    string DisplayName,
    bool DirectionIsUp,
    bool ComparisonIsAbsolute);