namespace Contracts;

public class DataPointWithValueDto
{
    public string Id {get; set;}
    public string OrganizationId {get; set;}
    public string DataPointKey {get; set;}
    public string DisplayName {get; set;}
    public bool DirectionIsUp {get; set;}

    public bool ComparisonIsAbsolute {get; set;}

    public double LatestValue {get; set;}

    public DataPointWithValueDto(
        string id,
        string organizationId,
        string dataPointKey,
        string displayName,
        double latestValue,
        bool directionIsUp = true,
        bool comparisonIsAbsolute = false
    )
    {
        Id = id;
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = displayName;
        LatestValue = latestValue;
        DirectionIsUp = directionIsUp;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }
}
