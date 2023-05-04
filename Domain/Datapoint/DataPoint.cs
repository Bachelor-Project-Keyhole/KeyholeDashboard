namespace Domain.Datapoint;

public class DataPoint
{ 
    public string Id { get; set; }
    public string OrganizationId { get; set; }
    public string Key { get; set; }
    public double Value { get; set; }
    
    public DataPoint(string organizationId, string key, double value)
    {
        Id = IdGenerator.GenerateId();
        OrganizationId = organizationId;
        Key = key;
        Value = value;
    }

    public DataPoint(string id, string organizationId, string key, double value)
    {
        Id = id;
        OrganizationId = organizationId;
        Key = key;
        Value = value;
    }
}