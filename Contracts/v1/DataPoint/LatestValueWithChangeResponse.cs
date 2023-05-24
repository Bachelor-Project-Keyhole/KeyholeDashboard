namespace Contracts.v1.DataPoint;

public class LatestValueWithChangeResponse
{
    public double LatestValue { get; set; }
    public double Change { get; set; }
    public bool DirectionIsUp { get; set; }
    public bool ComparisonIsAbsolute { get; set; }
}