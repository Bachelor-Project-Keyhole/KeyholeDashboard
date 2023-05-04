using MongoDB.Bson;

namespace Repository.Datapoint;

[BsonCollection("datapoints")]
public class DatapointEntity : Document
{
    public DatapointEntity(string organizationId, string key, double value)
    {
        OrganizationId = organizationId;
        Key = key;
        Value = value;
    }

    public string OrganizationId { get; set; }
    public string Key { get; set; }
    public double Value { get; set; }
}