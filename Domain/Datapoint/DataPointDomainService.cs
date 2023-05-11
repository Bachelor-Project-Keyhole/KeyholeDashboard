using Domain.Exceptions;
using Domain.RepositoryInterfaces;

namespace Domain.Datapoint;

public class DataPointDomainService : IDataPointDomainService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IDataPointRepository _dataPointRepository;
    private readonly IDataPointEntryRepository _dataPointEntryRepository;

    public DataPointDomainService(
        IOrganizationRepository organizationRepository,
        IDataPointRepository dataPointRepository,
        IDataPointEntryRepository dataPointEntryRepository)
    {
        _organizationRepository = organizationRepository;
        _dataPointRepository = dataPointRepository;
        _dataPointEntryRepository = dataPointEntryRepository;
    }

    public async Task<DataPoint[]> GetAllDataPoints(string organizationId)
    {
        await ValidateOrganization(organizationId);
        return await _dataPointRepository.GetAllDatapointForOrganization(organizationId);
    }

    public async Task AddDataPointEntry(DataPointEntry dataPointEntry)
    {
        await ValidateOrganization(dataPointEntry.OrganizationId);
        var dataPoint = await _dataPointRepository.FindDataPointByKey(dataPointEntry.Key, dataPointEntry.OrganizationId);
        if (dataPoint is null)
        {
            await CreateDataPoint(dataPointEntry.OrganizationId, dataPointEntry.Key);
        }
        await _dataPointEntryRepository.AddDataPointEntry(dataPointEntry);
    }

    private async Task CreateDataPoint(string organizationId, string key)
    {
        var dataPoint = new DataPoint(organizationId, key);
        await _dataPointRepository.CreateDataPoint(dataPoint);
    }

    public async Task<DataPointEntry[]> GetAllDataPointEntries(string organizationId, string key)
    {
        await ValidateOrganization(organizationId);

        var allDataPointEntries = await _dataPointEntryRepository.GetAllDataPointEntries(organizationId, key);
        if (allDataPointEntries.Length == 0)
        {
            throw new DataPointKeyNotFoundException($"Data point key with value: \'{organizationId}\' was not found");
        }
        return allDataPointEntries;
    }

    public async Task UpdateDataPoint(DataPoint dataPoint)
    {
        await ValidateOrganization(dataPoint.OrganizationId);

        var dataPointByKey = await _dataPointRepository.FindDataPointByKey(dataPoint.Key, dataPoint.OrganizationId);
        if (dataPointByKey is null)
        {
            throw new DataPointKeyNotFoundException($"Data point key with value: \'{dataPoint.Key}\' was not found");
        }

        if (dataPoint.Id != dataPointByKey.Id)
        {
            throw new EntityWithIdDoesNotExistException($"Data point with id: {dataPoint.Id} does not exist");
        }

        await _dataPointRepository.UpdateDataPoint(dataPoint);
    }

    private async Task ValidateOrganization(string organizationId)
    {
        var organizationExists = await _organizationRepository.OrganizationExists(organizationId);
        if (!organizationExists)
        {
            throw new OrganizationNotFoundException($"Organization with Id: {organizationId} was not found");
        }
    }
}