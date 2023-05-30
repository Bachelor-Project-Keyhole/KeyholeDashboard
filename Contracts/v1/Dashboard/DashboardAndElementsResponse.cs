namespace Contracts.v1.Dashboard;

public class DashboardAndElementsResponse
{
    public string DashboardId { get; set; }
    public string DashboardName { get; set; }
    public List<Placeholders> Placeholders { get; set; }
}

public class Placeholders
{
    public int PositionHeight { get; set; }
    public int PositionWidth { get; set; }
    public int SizeHeight { get; set; }
    public int SizeWidth { get; set; }
    public string TemplateId { get; set; }
    public List<ValueResponse> Values { get; set; }
    public Domain.Template.DisplayType DisplayType { get; set; }
    public double Change { get; set; }
    public bool Comparison { get; set; }
    public double LatestValue { get; set; }
    public bool IsDirectionUp { get; set; }
    public string DisplayName { get; set; }
    
}

public class ValueResponse
{
    public double Value { get; set; }
    public DateTime? Time { get; set; }
}