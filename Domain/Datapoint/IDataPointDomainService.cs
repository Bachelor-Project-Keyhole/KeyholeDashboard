namespace Domain.Datapoint;

public interface IDataPointDomainService
{
    Task<DataPoint[]> GetAllDataPoints(string organizationId);
    Task AddDataPointEntry(string dataPointKey, double value, string apiKey);
    Task<DataPointEntry[]> GetAllDataPointEntries(string organizationId, string dataPointKey);
    Task UpdateDataPoint(DataPoint dataPoint);
    Task<DataPointEntry> GetLatestDataPointEntry(string organizationId, string dataPointKey);
    Task<DataPoint> CreateDataPoint(DataPoint dataPoint);
    Task<DataPoint> GetDataPointById(string dataPointId);
    Task<DataPointEntry[]> GetDataPointEntries(string organizationId, string dataPointKey, DateTime periodDateTime);
    Task<double> CalculateChangeOverTime(DataPoint dataPoint, DateTime periodDateTime);
    Task AddHistoricDataPointEntries(DataPointEntry[] dataPointEntries, string apiKey);
    Task AddDataPointEntries(DataPointEntry[] dataPointEntries, string apiKey);
}