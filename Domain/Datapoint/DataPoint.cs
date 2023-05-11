namespace Domain.Datapoint;

public class DataPoint
{
    public string? Id { get; set; }
    public string OrganizationId { get; set; }
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public bool DirectionIsUp { get; set; }
    public bool ComparisonIsAbsolute { get; set; }
    
    public DataPoint(string organizationId, string key, bool directionIsUp = true, bool comparisonIsAbsolute = false)
    {
        OrganizationId = organizationId;
        Key = key;
        DisplayName = key;
        DirectionIsUp = directionIsUp;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }

    public DataPoint(string organizationId, string key,  string displayName, bool directionIsUp = true, bool comparisonIsAbsolute = false)
    {
        OrganizationId = organizationId;
        Key = key;
        DirectionIsUp = directionIsUp;
        DisplayName = displayName;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }
}
