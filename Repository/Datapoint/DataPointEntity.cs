using Domain.Datapoint;
using MongoDB.Bson;

namespace Repository.Datapoint;

[BsonCollection("datapoints")]
public class DataPointEntity : Document
{
    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public string DisplayName { get; set; }
    public bool IsDirectionUp { get; set; }
    public bool IsComparisonAbsolute { get; set; }
    public double LatestValue { get; set; }

    public Formula Formula { get; set; } = new() { Operation = MathOperation.None };

    public DataPointEntity(
        string organizationId,
        string dataPointKey,
        string displayName,
        bool isDirectionUp = false,
        bool isComparisonAbsolute = false,
        double latestValue = 0)
    {
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = displayName;
        IsDirectionUp = isDirectionUp;
        IsComparisonAbsolute = isComparisonAbsolute;
        LatestValue = latestValue;
    }
    
    public DataPointEntity(
        string id,
        string organizationId,
        string dataPointKey,
        string displayName,
        bool isDirectionUp = false,
        bool isComparisonAbsolute = false,
        double latestValue = 0)
    {
        Id = new ObjectId(id);
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = displayName;
        IsDirectionUp = isDirectionUp;
        IsComparisonAbsolute = isComparisonAbsolute;
        LatestValue = latestValue;
    }
}