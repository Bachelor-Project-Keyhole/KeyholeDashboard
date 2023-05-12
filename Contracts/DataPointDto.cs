namespace Contracts;

public record DataPointDto(
    string Id,
    string OrganizationId,
    string Key, 
    string DisplayName,
    bool DirectionIsUp,
    bool ComparisonIsAbsolute);