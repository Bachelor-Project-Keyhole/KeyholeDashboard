using System.ComponentModel.DataAnnotations;

namespace Contracts.@public;

public class PushDataPointEntryRequest
{
    [Required] public string DataPointKey { get; set; }

    [Required] public double Value { get; set; }

    public PushDataPointEntryRequest(string dataPointKey, double value)
    {
        DataPointKey = dataPointKey;
        Value = value;
    }
}