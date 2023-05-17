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
    
    public DataPoint(string organizationId, string dataPointKey, bool directionIsUp = true, bool comparisonIsAbsolute = false)
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
        switch (Formula.Operation)
        {
            case MathOperation.Add: 
                LatestValue = value + Formula.Factor;
                break;
            case MathOperation.Multiply:
                LatestValue = value * Formula.Factor;
                break;
            case MathOperation.Divide:
                LatestValue = value / Formula.Factor;
                break;
            case MathOperation.Subtract:
                LatestValue = value - Formula.Factor;
                break;
            default:
                LatestValue = value;
                break;
        }
    }
}
