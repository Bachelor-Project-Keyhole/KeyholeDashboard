namespace Domain.Datapoint;

public class LatestValueWithChange
{
    public string DataPointKey { get; set; }
    public string DisplayName { get; set; }
    public double LatestValue { get; set; }
    public double Change { get; set; }
    public bool IsDirectionUp { get; set; }
    public bool IsComparisonAbsolute { get; set; }
}