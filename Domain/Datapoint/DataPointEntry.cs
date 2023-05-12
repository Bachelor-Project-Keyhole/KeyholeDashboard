namespace Domain.Datapoint;

public class DataPointEntry
{
    public string Id { get; set; }
    public string OrganizationId { get; set; }
    public string Key { get; set; }
    public double Value { get; set; }
    public DateTime? Time { get; set; }

    public DataPointEntry(string id, string organizationId, string key, double value, DateTime? time)
    {
        Id = id;
        OrganizationId = organizationId;
        Key = key;
        Value = value;
        Time = time;
    }
    
    public DataPointEntry(string organizationId, string key, double value, DateTime? time)
    {
        Id = IdGenerator.GenerateId();
        OrganizationId = organizationId;
        Key = key;
        Value = value;
        Time = time;
    }
}