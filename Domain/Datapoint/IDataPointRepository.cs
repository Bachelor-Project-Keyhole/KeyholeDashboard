namespace Domain.Datapoint;

public interface IDataPointRepository
{
    Task<DataPoint[]> GetAllDatapointForOrganization(string organizationId);
    Task<DataPoint?> FindDataPointByKey(string key, string organizationId);
    Task CreateDataPoint(DataPoint dataPoint);
    Task UpdateDataPoint(DataPoint dataPoint);
}