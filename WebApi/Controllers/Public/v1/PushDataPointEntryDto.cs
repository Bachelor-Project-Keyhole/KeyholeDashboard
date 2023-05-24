using System.ComponentModel.DataAnnotations;

namespace WebApi.Controllers.Public.v1;

public class PushDataPointEntryDto
{
    [Required] public string DataPointKey { get; set; }

    [Required] public double Value { get; set; }

    public PushDataPointEntryDto(string dataPointKey, double value)
    {
        DataPointKey = dataPointKey;
        Value = value;
    }
}