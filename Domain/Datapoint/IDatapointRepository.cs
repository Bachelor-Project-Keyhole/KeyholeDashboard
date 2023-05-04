namespace Domain.Datapoint;

public interface IDatapointRepository
{
    Task<DataPoint[]> GetAllDatapointForOrganization(string organizationId);
}