using Domain.Datapoint;

namespace Domain.Template;

public interface ITemplateDomainService
{
    Task<DataPointEntry[]> GetDataForTemplate(string organizationId, string dataPointId, string displayType,
        int timeSpan, TimeUnit timeUnit);

    Task<(double LatestValue, double Change, bool DirectionIsUp, bool ComparisonIsAbsolute)> GetLatestValueWithChange(
        string dataPointId, int timeSpan, TimeUnit timeUnit);

    Task<Template> GetTemplateById(string id);
    Task<List<Template>> GetAllByDashboardId(string dashboardId);
    Task<Template> CreateTemplate(Template template);
    Task<Template> Update(Template template);
    Task RemoveTemplate(string id);
    Task RemoveAllTemplatesWithDashboardId(string dashboardId);
}