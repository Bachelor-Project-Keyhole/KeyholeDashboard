namespace Contracts.v1.DataPoint;

public class CreateDataPointRequest
{
    public CreateDataPointRequest(string organizationId, string dataPointKey, string displayName, FormulaDto formula,
        bool isDirectionUp, bool isComparisonAbsolute)
    {
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = displayName;
        Formula = formula;
        IsDirectionUp = isDirectionUp;
        IsComparisonAbsolute = isComparisonAbsolute;
    }

    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public string DisplayName { get; set; }
    public FormulaDto Formula { get; set; }
    public bool IsDirectionUp { get; set; }
    public bool IsComparisonAbsolute { get; set; }
}