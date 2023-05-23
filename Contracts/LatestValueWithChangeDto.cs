namespace Contracts;

public class LatestValueWithChangeDto
{
    public double LatestValue { get; set; }
    public double Change { get; set; }
    public bool DirectionIsUp { get; set; }
    public bool ComparisonIsAbsolute { get; set; }
}