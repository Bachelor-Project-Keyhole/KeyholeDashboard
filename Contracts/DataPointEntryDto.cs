namespace Contracts;

public record DataPointEntryDto(string OrganizationId, string DataPointKey, double Value, DateTime? Time)
{
}