using Domain.Datapoint;

namespace Domain.Template;

public class TemplateDomainService : ITemplateDomainService
{
    private readonly IDataPointDomainService _dataPointDomainService;

    public TemplateDomainService(IDataPointDomainService dataPointDomainService)
    {
        _dataPointDomainService = dataPointDomainService;
    }

    public async Task<DataPointEntry[]> GetDataForTemplate(string organizationId, string dataPointId,
        string displayType, int timeSpan, TimeUnit timeUnit)
    {
        var dataPoint = await _dataPointDomainService.GetDataPointById(dataPointId);
        var periodStartDate = TimeSpanConverter.CalculatePeriodBoundary(timeSpan, timeUnit);
        var dataPointEntries =
            await _dataPointDomainService.GetDataPointEntries(organizationId, dataPoint.DataPointKey, periodStartDate);
        CalculateEntryValuesBasedOnFormula(dataPoint, dataPointEntries);
        return dataPointEntries;
    }

    public async Task<(double LatestValue, double Change, bool DirectionIsUp, bool ComparisonIsAbsolute)>
        GetLatestValueWithChange(string dataPointId, int timeSpan,
            TimeUnit timeUnit)
    {
        var dataPoint = await _dataPointDomainService.GetDataPointById(dataPointId);
        var endOfPeriod = TimeSpanConverter.CalculatePeriodBoundary(timeSpan, timeUnit);
        var change = await _dataPointDomainService.CalculateChangeOverTime(dataPoint, endOfPeriod);
        return (dataPoint.LatestValue, change, dataPoint.DirectionIsUp, dataPoint.ComparisonIsAbsolute);
    }

    private void CalculateEntryValuesBasedOnFormula(DataPoint dataPoint, DataPointEntry[] dataPointEntries)
    {
        foreach (var dataPointEntry in dataPointEntries)
        {
            dataPointEntry.Value = dataPoint.CalculateEntryValueWithFormula(dataPointEntry.Value);
        }
    }
}