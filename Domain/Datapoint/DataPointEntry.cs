namespace Domain.Datapoint;

public class DataPointEntry
{
    public string Id { get; set; }
    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public double Value { get; set; }
    public DateTime? Time { get; set; }
}