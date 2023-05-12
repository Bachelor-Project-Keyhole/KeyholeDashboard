namespace Domain.Datapoint;

public interface IDataPointEntryRepository
{
    Task AddDataPointEntry(DataPointEntry dataPointEntry);
    Task<DataPointEntry[]> GetAllDataPointEntries(string organizationId, string key);
}