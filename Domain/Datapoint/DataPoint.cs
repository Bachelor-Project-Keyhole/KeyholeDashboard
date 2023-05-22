namespace Domain.Datapoint;

public class DataPoint
{
    public string Id { get; set; }

    public string OrganizationId { get; set; }

    public string DataPointKey { get; set; }

    public string DisplayName { get; set; }

    public bool DirectionIsUp { get; set; }

    public bool ComparisonIsAbsolute { get; set; }

    public double LatestValue { get; set; }

    public Formula Formula { get; set; } = new() { Operation = MathOperation.None };

    public DataPoint(string organizationId, string dataPointKey, bool directionIsUp = true,
        bool comparisonIsAbsolute = false)
    {
        Id = IdGenerator.GenerateId();
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = dataPointKey;
        DirectionIsUp = directionIsUp;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }

    public DataPoint(
        string id,
        string organizationId,
        string dataPointKey,
        string displayName,
        double latestValue,
        Formula formula,
        bool directionIsUp = true,
        bool comparisonIsAbsolute = false)
    {
        Id = id;
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DirectionIsUp = directionIsUp;
        DisplayName = displayName;
        LatestValue = latestValue;
        Formula = formula;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }

    public void SetLatestValueBasedOnFormula(double value)
    {
        LatestValue = CalculateValue(value);
    }

    public double CalculateEntryValueWithFormula(double value)
    {
        return CalculateValue(value);
    }

    private double CalculateValue(double value)
    {
        switch (Formula.Operation)
        {
            case MathOperation.Add:
                return value + Formula.Factor;
            case MathOperation.Multiply:
                return value * Formula.Factor;
            case MathOperation.Divide:
                return value / Formula.Factor;
            case MathOperation.Subtract:
                return value - Formula.Factor;
            default:
                return value;
        }
    }

    //Dividing with zero not only does not throw but also gets correct result
    public double CalculateChangeOverTime(double value)
    {
        // % increase = Increase ÷ Original Number × 100.
        //  If result is a negative number, then this is a percentage decrease.
        return ComparisonIsAbsolute ? LatestValue - value : (LatestValue - value) / value * 100;
    }
}