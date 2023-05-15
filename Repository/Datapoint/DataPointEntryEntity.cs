using MongoDB.Bson.Serialization.Attributes;

namespace Repository.Datapoint;


[BsonCollection("datapoint-entries")]
public class DataPointEntryEntity : Document
{
    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public double Value { get; set; }
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime Time { get; set; }

    public DataPointEntryEntity(string organizationId, string dataPointKey, double value, DateTime time)
    {
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        Value = value;
        Time = time;
    }
}