namespace Domain.Datapoint;

public interface IDataPointEntryRepository
{
    Task AddDataPointEntry(DataPointEntry dataPointEntry);
    Task AddDataPointEntries(DataPointEntry[] dataPointEntry);
    Task<DataPointEntry[]> GetAllDataPointEntries(string organizationId, string key);
    Task<DataPointEntry?> GetLatestDataPointEntry(string organizationId, string dataPointKey);
    Task<IEnumerable<DataPointEntry>> GetDataPointEntries(string organizationId, string dataPointKey,
        DateTime periodDateTime);
    Task<DataPointEntry?> GetDataPointEntryFromPreviousPeriod(string organizationId, string dataPointKey, DateTime endOfPeriod);
}