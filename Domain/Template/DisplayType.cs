using System.Text.Json.Serialization;

namespace Domain.Template;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisplayType
{
    LineChart,
    BarChart,
    Numeric
}