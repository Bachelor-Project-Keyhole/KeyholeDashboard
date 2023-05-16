namespace Repository.Datapoint;

[BsonCollection("datapoints")]
public class DataPointEntity : Document
{
    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public string DisplayName { get; set; }
    public bool DirectionIsUp { get; set; }
    public bool ComparisonIsAbsolute { get; set; }

    public DataPointEntity(
        string organizationId,
        string dataPointKey,
        string displayName,
        bool directionIsUp = false,
        bool comparisonIsAbsolute = false)
    {
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = displayName;
        DirectionIsUp = directionIsUp;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }
}