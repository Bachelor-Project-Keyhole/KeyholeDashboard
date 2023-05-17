namespace Domain.Datapoint;

public interface IDataPointDomainService
{
    Task<DataPoint[]> GetAllDataPoints(string organizationId);
    Task AddDataPointEntry(DataPointEntry dataPointEntry);
    Task<DataPointEntry[]> GetAllDataPointEntries(string organizationId, string dataPointKey);
    Task UpdateDataPoint(DataPoint dataPoint);
    Task<DataPointEntry> GetLatestDataPointEntry(string organizationId, string dataPointKey);
    Task<DataPoint> CreateDataPoint(DataPoint dataPoint);
}