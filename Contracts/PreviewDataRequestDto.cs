namespace Contracts;

public class PreviewDataRequestDto
{
    public string OrganizationId { get; set; }
    public string DataPointId { get; set; }
    public string DisplayType { get; set; }
    public int TimeSpanInDays { get; set; }
}