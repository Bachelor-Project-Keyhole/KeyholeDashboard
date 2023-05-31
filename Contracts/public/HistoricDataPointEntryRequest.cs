using System.ComponentModel.DataAnnotations;

namespace Contracts.@public;

public class HistoricDataPointEntryRequest
{
    [Required] public string DataPointKey { get; set; }
    [Required] public double Value { get; set; }
    [Required] public DateTime Time { get; set; }

    public HistoricDataPointEntryRequest(string dataPointKey, double value, DateTime time)
    {
        DataPointKey = dataPointKey;
        Value = value;
        Time = time;
    }
}