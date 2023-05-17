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

    public async Task<DataPoint> CreateDataPoint(DataPoint dataPoint)
    {
        await ValidateOrganization(dataPoint.OrganizationId);
        await UpdateDataPointLatestValue(dataPoint);
        dataPoint.Id = IdGenerator.GenerateId();
        await _dataPointRepository.CreateDataPoint(dataPoint);
        return dataPoint;
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

    public async Task<DataPointEntry[]> GetAllDataPointEntries(string organizationId, string dataPointKey)
    {
        await ValidateOrganization(organizationId);

        var allDataPointEntries = await _dataPointEntryRepository.GetAllDataPointEntries(organizationId, dataPointKey);
        if (allDataPointEntries.Length == 0)
        {
            throw new DataPointKeyNotFoundException(dataPointKey);
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
            throw new DataPointKeyNotFoundException(dataPointKey);
        }

        return latestDataPointEntry;
    }

    private async Task CreateDataPoint(string organizationId, string key, double dataPointLatestValue)
    {
        var dataPoint = new DataPoint(organizationId, key);
        dataPoint.SetLatestValueBasedOnFormula(dataPointLatestValue);
        await _dataPointRepository.CreateDataPoint(dataPoint);
    }

    private async Task UpdateDataPointLatestValue(DataPoint dataPoint)
    {
        var latestDataPointEntry =
            await _dataPointEntryRepository.GetLatestDataPointEntry(dataPoint.OrganizationId, dataPoint.DataPointKey);
        if (latestDataPointEntry is null)
        {
            throw new DataPointKeyNotFoundException(dataPoint.DataPointKey);
        }

        dataPoint.SetLatestValueBasedOnFormula(latestDataPointEntry.Value);
    }

    private async Task ValidateOrganization(string organizationId)
    {
        var organizationExists = await _organizationRepository.OrganizationExists(organizationId);
        if (!organizationExists)
        {
            throw new OrganizationNotFoundException(organizationId);
        }
    }
}