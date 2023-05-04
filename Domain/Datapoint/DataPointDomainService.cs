using Domain.RepositoryInterfaces;

namespace Domain.Datapoint;

public class DataPointDomainService : IDataPointDomainService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IDatapointRepository _datapointRepository;

    public DataPointDomainService(IOrganizationRepository organizationRepository, IDatapointRepository datapointRepository)
    {
        _organizationRepository = organizationRepository;
        _datapointRepository = datapointRepository;
    }

    public async Task<DataPoint[]> GetAllDataPoints(string organizationId)
    {
        var organizationExists = await _organizationRepository.OrganizationExists(organizationId);
        if (!organizationExists)
        {
            throw new OrganizationNotFoundException($"Organization with {organizationId} was not found");
        }
        return await _datapointRepository.GetAllDatapointForOrganization(organizationId);
    }
}

public class OrganizationNotFoundException : Exception
{
    public OrganizationNotFoundException(string message): base(message)
    {}
}