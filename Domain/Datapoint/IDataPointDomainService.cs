namespace Domain.Datapoint;

public interface IDataPointDomainService
{
    Task<DataPoint[]> GetAllDataPoints(string organizationId);
}