using MongoDB.Bson.Serialization.Attributes;

namespace Repository.Datapoint;


[BsonCollection("datapoint-entries")]
public class DataPointEntryEntity : Document
{
    public string OrganizationId { get; set; }
    public string Key { get; set; }
    public double Value { get; set; }
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime Time { get; set; }

    public DataPointEntryEntity(string organizationId, string key, double value, DateTime time)
    {
        OrganizationId = organizationId;
        Key = key;
        Value = value;
        Time = time;
    }
}