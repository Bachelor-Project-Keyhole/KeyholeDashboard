using Contracts.@public;

namespace Application.DataPoint;

public interface IDataPointApplicationService
{
    Task AddDataPointEntry(string dataPointKey, double value, string apiKey);
    Task AddDataPointEntries(PushDataPointEntryRequest[] dataPointEntryDtos, string apiKey);
    Task AddHistoricDataPointEntries(HistoricDataPointEntryRequest[] dataPointEntryDtos, string apiKey);
}