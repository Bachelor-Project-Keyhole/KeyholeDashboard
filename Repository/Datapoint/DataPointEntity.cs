using Domain.Datapoint;
using MongoDB.Bson;

namespace Repository.Datapoint;

[BsonCollection("datapoints")]
public class DataPointEntity : Document
{
    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public string DisplayName { get; set; }
    public bool DirectionIsUp { get; set; }
    public bool ComparisonIsAbsolute { get; set; }
    public double LatestValue { get; set; }

    public Formula Formula { get; set; } = new() { Operation = MathOperation.None };

    public DataPointEntity(
        string organizationId,
        string dataPointKey,
        string displayName,
        bool directionIsUp = false,
        bool comparisonIsAbsolute = false,
        double latestValue = 0)
    {
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = displayName;
        DirectionIsUp = directionIsUp;
        ComparisonIsAbsolute = comparisonIsAbsolute;
        LatestValue = latestValue;
    }
    
    public DataPointEntity(
        string id,
        string organizationId,
        string dataPointKey,
        string displayName,
        bool directionIsUp = false,
        bool comparisonIsAbsolute = false,
        double latestValue = 0)
    {
        Id = new ObjectId(id);
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = displayName;
        DirectionIsUp = directionIsUp;
        ComparisonIsAbsolute = comparisonIsAbsolute;
        LatestValue = latestValue;
    }
}