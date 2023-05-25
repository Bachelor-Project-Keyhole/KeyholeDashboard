using Domain.Template;

namespace Contracts.v1.Template;

public class TemplateResponse
{
    public string Id { get; set; }
    public string DashboardId { get; set; }
    public string DatapointId { get; set; }
    public DisplayType DisplayType { get; set; }
    public int TimePeriod { get; set; }
    public TimeUnit TimeUnit { get; set; }
    public int PositionWidth { get; set; }
    public int PositionHeight { get; set; }
    public int SizeWidth { get; set; }
    public int SizeHeight { get; set; }
}