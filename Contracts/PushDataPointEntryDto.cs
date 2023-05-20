namespace Contracts;

public record PushDataPointEntryDto(string OrganizationId, string DataPointKey, double Value, DateTime? Time)
{
}