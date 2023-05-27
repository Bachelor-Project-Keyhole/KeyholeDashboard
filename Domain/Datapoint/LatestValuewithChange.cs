namespace Domain.Datapoint;

public class LatestValuewithChange
{
    public string DataPointKey { get; set; }
    public string DisplayName { get; set; }
    public double LatestValue { get; set; }
    public double Change { get; set; }
    public bool DirectionIsUp { get; set; }
    public bool ComparisonIsAbsolute { get; set; }
}