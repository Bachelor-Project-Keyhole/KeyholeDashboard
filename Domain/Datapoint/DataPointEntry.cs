namespace Domain.Datapoint;

public class DataPointEntry
{
    public string OrganizationId { get; set; }
    public string Key { get; set; }
    public double Value { get; set; }
    public DateTime? Time { get; set; }

    public DataPointEntry(string organizationId, string key, double value, DateTime? time)
    {
        OrganizationId = organizationId;
        Key = key;
        Value = value;
        Time = time;
    }
}