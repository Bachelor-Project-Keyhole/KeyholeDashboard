namespace Domain.Datapoint;

public interface IDataPointEntryRepository
{
    Task AddDataPointEntry(DataPointEntry dataPointEntry);
    Task<DataPointEntry[]> GetAllDataPointEntries(string organizationId, string key);
    Task<DataPointEntry?> GetLatestDataPointEntry(string organizationId, string dataPointKey);
    Task<IEnumerable<DataPointEntry>> GetDataPointEntries(string organizationId, string dataPointKey, int timeSpanInDays);
}