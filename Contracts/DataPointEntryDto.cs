namespace Contracts;

public record DataPointEntryDto(string OrganizationId, string Key, double Value, DateTime? Time)
{
}