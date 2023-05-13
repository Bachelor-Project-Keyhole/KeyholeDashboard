namespace Domain.Datapoint;

public class DataPoint
{
    public string? Id { get; set; }
    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public string DisplayName { get; set; }
    public bool DirectionIsUp { get; set; }
    public bool ComparisonIsAbsolute { get; set; }
    
    public DataPoint(string organizationId, string dataPointKey, bool directionIsUp = true, bool comparisonIsAbsolute = false)
    {
        Id = IdGenerator.GenerateId();
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = dataPointKey;
        DirectionIsUp = directionIsUp;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }

    public DataPoint(string id, string organizationId, string dataPointKey,  string displayName, bool directionIsUp = true, bool comparisonIsAbsolute = false)
    {
        Id = id;
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DirectionIsUp = directionIsUp;
        DisplayName = displayName;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }
}
