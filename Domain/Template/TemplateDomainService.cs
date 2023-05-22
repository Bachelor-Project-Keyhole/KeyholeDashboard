using System.ComponentModel;
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

    public async Task<(double LatestValue, double Change)> GetLatestValueWithChange(string dataPointId, int timeSpan,
        TimeUnit timeUnit)
    {
        var timeSpanInDays = ConvertTimeSpanToDays(timeSpan, timeUnit);
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

    private DateTime ConvertTimeSpanToDays(int timespan, TimeUnit timeUnit)
    {
        var result = DateTime.Today;
        switch (timeUnit)
        {
            case TimeUnit.Day:
                return result.AddDays(-timespan);
            case TimeUnit.Week:
                return result.AddDays(-timespan*7);
            case TimeUnit.Month:
                return result.AddMonths(-timespan);
            case TimeUnit.Year:
                return result.AddYears(-timespan);
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}