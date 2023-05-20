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

    private void CalculateEntryValuesBasedOnFormula(DataPoint dataPoint, DataPointEntry[] dataPointEntries)
    {
        foreach (var dataPointEntry in dataPointEntries)
        {
            dataPointEntry.Value = dataPoint.CalculateEntryValueWithFormula(dataPointEntry.Value);
        }
    }
}