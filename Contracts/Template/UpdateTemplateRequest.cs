namespace Contracts.Template;

public class UpdateTemplateRequest
{
    public string Id { get; set; }
    public string? DatapointId { get; set; }
    public Domain.Template.DisplayType? DisplayType { get; set; }
    public int? TimePeriod { get; set; }
    public Domain.Template.TimeUnit? TimeUnit { get; set; }
    public int? PositionWidth { get; set; }
    public int? PositionHeight { get; set; }
    public int? SizeWidth { get; set; }
    public int? SizeHeight { get; set; }
}