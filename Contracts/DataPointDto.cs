namespace Contracts;

public record DataPointDto(string Id, string OrganizationId, string Key, double Value, DateTime Time);
