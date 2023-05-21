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
        string displayType, int timeSpanInDays)
    {
        var dataPoint = await _dataPointDomainService.GetDataPointById(dataPointId);
        var dataPointEntries =
            await _dataPointDomainService.GetDataPointEntries(organizationId, dataPoint.DataPointKey, timeSpanInDays);
        CalculateEntryValuesBasedOnFormula(dataPoint, dataPointEntries);
        return dataPointEntries;
    }

    public async Task<(double LatestValue, double Change)> GetLatestValueWithChange(string organizationId, string dataPointId, int timeSpanInDays)
    {
        var dataPoint = await _dataPointDomainService.GetDataPointById(dataPointId);
        var change = await _dataPointDomainService.CalculateChangeOverTime(dataPoint, timeSpanInDays);
        return (dataPoint.LatestValue, change);
    }

    private void CalculateEntryValuesBasedOnFormula(DataPoint dataPoint, DataPointEntry[] dataPointEntries)
    {
        foreach (var dataPointEntry in dataPointEntries)
        {
            dataPointEntry.Value = dataPoint.CalculateEntryValueWithFormula(dataPointEntry.Value);
        }
    }
}