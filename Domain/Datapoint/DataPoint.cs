namespace Domain.Datapoint;

public class DataPoint
{
    public string Id { get; set; }

    public string OrganizationId { get; set; }

    public string DataPointKey { get; set; }

    public string DisplayName { get; set; }

    public bool IsDirectionUp { get; set; }

    public bool IsComparisonAbsolute { get; set; }

    public double LatestValue { get; set; }

    public Formula Formula { get; set; } = new() { Operation = MathOperation.None };

    public DataPoint(string organizationId, string dataPointKey, bool isDirectionUp = true,
        bool isComparisonAbsolute = false)
    {
        Id = IdGenerator.GenerateId();
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = dataPointKey;
        IsDirectionUp = isDirectionUp;
        IsComparisonAbsolute = isComparisonAbsolute;
    }

    public DataPoint(
        string id,
        string organizationId,
        string dataPointKey,
        string displayName,
        double latestValue,
        Formula formula,
        bool isDirectionUp = true,
        bool isComparisonAbsolute = false)
    {
        Id = id;
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        IsDirectionUp = isDirectionUp;
        DisplayName = displayName;
        LatestValue = latestValue;
        Formula = formula;
        IsComparisonAbsolute = isComparisonAbsolute;
    }

    public void SetLatestValueBasedOnFormula(double value)
    {
        LatestValue = Math.Round(CalculateValue(value), 3);
    }

    public double CalculateEntryValueWithFormula(double value)
    {
        return Math.Round(CalculateValue(value), 3);
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

    public double CalculateChangeOverTime(double originalValue)
    {
        if (Math.Abs(originalValue - LatestValue) < 0.0001)
        {
            return 0;
        }
        if (!IsComparisonAbsolute && originalValue == 0)
        {
            return LatestValue > 0 ? 100 : -100;
        }

        // % increase = Increase ÷ Original Number × 100.
        //  If result is a negative number, then this is a decrease.
        return IsComparisonAbsolute
            ? LatestValue - originalValue
            : (LatestValue - originalValue) / Math.Abs(originalValue) * 100;
    }
}