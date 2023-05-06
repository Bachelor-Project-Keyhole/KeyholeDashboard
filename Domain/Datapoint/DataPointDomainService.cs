using Domain.Exceptions;
using Domain.RepositoryInterfaces;

namespace Domain.Datapoint;

public class DataPointDomainService : IDataPointDomainService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IDatapointRepository _datapointRepository;
    private readonly IDataPointEntryRepository _dataPointEntryRepository;

    public DataPointDomainService(
        IOrganizationRepository organizationRepository,
        IDatapointRepository datapointRepository,
        IDataPointEntryRepository dataPointEntryRepository)
    {
        _organizationRepository = organizationRepository;
        _datapointRepository = datapointRepository;
        _dataPointEntryRepository = dataPointEntryRepository;
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

    public async Task AddDataPointEntry(DataPointEntry dataPointEntry)
    {
        var organizationExists = await _organizationRepository.OrganizationExists(dataPointEntry.OrganizationId);
        if (!organizationExists)
        {
            throw new OrganizationNotFoundException($"Organization with {dataPointEntry.OrganizationId} was not found");
        }
        await _dataPointEntryRepository.AddDataPointEntry(dataPointEntry);
    }
}