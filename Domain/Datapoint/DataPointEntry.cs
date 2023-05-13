namespace Domain.Datapoint;

public class DataPointEntry
{
    public string Id { get; set; }
    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public double Value { get; set; }
    public DateTime? Time { get; set; }

    public DataPointEntry(string id, string organizationId, string dataPointKey, double value, DateTime? time)
    {
        Id = id;
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        Value = value;
        Time = time;
    }
    
    public DataPointEntry(string organizationId, string dataPointKey, double value, DateTime? time)
    {
        Id = IdGenerator.GenerateId();
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        Value = value;
        Time = time;
    }
}