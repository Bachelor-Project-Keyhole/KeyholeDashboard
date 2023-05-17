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
        var dataPoints =
            await _dataPointRepository.FindDataPointsByKey(dataPointEntry.DataPointKey, dataPointEntry.OrganizationId);
        if (dataPoints.Length == 0)
        {
            await CreateDataPoint(dataPointEntry.OrganizationId, dataPointEntry.DataPointKey, dataPointEntry.Value);
        }
        else
        {
            foreach (var dataPoint in dataPoints)
            {
                dataPoint.SetLatestValueBasedOnFormula(dataPointEntry.Value);
                await _dataPointRepository.UpdateDataPoint(dataPoint);
            }
        }

        await _dataPointEntryRepository.AddDataPointEntry(dataPointEntry);
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

        // Update latest value to match the newest formula
        await UpdateDataPointLatestValue(dataPoint);
        await _dataPointRepository.UpdateDataPoint(dataPoint);
    }

    public async Task<DataPointEntry> GetLatestDataPointEntry(string organizationId, string dataPointKey)
    {
        await ValidateOrganization(organizationId);
        var latestDataPointEntry =
            await _dataPointEntryRepository.GetLatestDataPointEntry(organizationId, dataPointKey);
        if (latestDataPointEntry is null)
        {
            throw new DataPointKeyNotFoundException($"Data point key with value: \'{organizationId}\' was not found");
        }

        return latestDataPointEntry;
    }

    private async Task CreateDataPoint(string organizationId, string key, double dataPointLatestValue)
    {
        var dataPoint = new DataPoint(organizationId, key)
        {
            LatestValue = dataPointLatestValue
        };
        await _dataPointRepository.CreateDataPoint(dataPoint);
    }

    private async Task UpdateDataPointLatestValue(DataPoint dataPoint)
    {
        var latestDataPointEntry = await GetLatestDataPointEntry(dataPoint.OrganizationId, dataPoint.DataPointKey);
        dataPoint.SetLatestValueBasedOnFormula(latestDataPointEntry.Value);
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