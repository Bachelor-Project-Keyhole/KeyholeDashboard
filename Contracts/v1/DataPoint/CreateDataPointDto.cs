namespace Contracts.v1.DataPoint;

public class CreateDataPointDto
{
    public CreateDataPointDto(string organizationId, string dataPointKey, string displayName, FormulaDto formula,
        bool directionIsUp, bool comparisonIsAbsolute)
    {
        OrganizationId = organizationId;
        DataPointKey = dataPointKey;
        DisplayName = displayName;
        Formula = formula;
        DirectionIsUp = directionIsUp;
        ComparisonIsAbsolute = comparisonIsAbsolute;
    }

    public CreateDataPointDto()
    {
        
    }

    public string OrganizationId { get; set; }
    public string DataPointKey { get; set; }
    public string DisplayName { get; set; }
    public FormulaDto Formula { get; set; }
    public bool DirectionIsUp { get; set; }
    public bool ComparisonIsAbsolute { get; set; }
}