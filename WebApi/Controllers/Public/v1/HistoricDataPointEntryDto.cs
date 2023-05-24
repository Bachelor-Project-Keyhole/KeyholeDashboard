using System.ComponentModel.DataAnnotations;

namespace WebApi.Controllers.Public.v1;

public class HistoricDataPointEntryDto
{
    [Required] public string DataPointKey { get; set; }
    [Required] public double Value { get; set; }
    [Required] public DateTime Time { get; set; }

    public HistoricDataPointEntryDto(string dataPointKey, double value, DateTime time)
    {
        DataPointKey = dataPointKey;
        Value = value;
        Time = time;
    }
}