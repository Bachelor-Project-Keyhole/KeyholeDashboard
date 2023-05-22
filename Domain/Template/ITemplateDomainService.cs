using Domain.Datapoint;

namespace Domain.Template;

public interface ITemplateDomainService
{
    Task<DataPointEntry[]> GetDataForTemplate(string organizationId, string dataPointId, string displayType,
        int timeSpanInDays);

    Task<(double LatestValue, double Change)> GetLatestValueWithChange(string dataPointId, int timeSpan);
}