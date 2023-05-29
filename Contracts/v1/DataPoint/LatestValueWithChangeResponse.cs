namespace Contracts.v1.DataPoint;

public class LatestValueWithChangeResponse
{
    public double LatestValue { get; set; }
    public double Change { get; set; }
    public bool IsDirectionUp { get; set; }
    public bool IsComparisonAbsolute { get; set; }
}