using Domain.Exceptions;
using Domain.Organization;

namespace Domain.Datapoint;

public class DataPointDomainService : IDataPointDomainService
{
    private readonly IOrganizationDomainService _organizationDomainService;
    private readonly IDataPointRepository _dataPointRepository;
    private readonly IDataPointEntryRepository _dataPointEntryRepository;

    public DataPointDomainService(
        IDataPointRepository dataPointRepository,
        IDataPointEntryRepository dataPointEntryRepository,
        IOrganizationDomainService organizationDomainService)
    {
        _dataPointRepository = dataPointRepository;
        _dataPointEntryRepository = dataPointEntryRepository;
        _organizationDomainService = organizationDomainService;
    }

    public async Task<DataPoint> CreateDataPoint(DataPoint dataPoint)
    {
        await ValidateOrganization(dataPoint.OrganizationId);
        await UpdateDataPointLatestValue(dataPoint);
        dataPoint.Id = IdGenerator.GenerateId();
        await _dataPointRepository.CreateDataPoint(dataPoint);
        return dataPoint;
    }

    public async Task<DataPoint> GetDataPointById(string dataPointId)
    {
        var dataPoint = await _dataPointRepository.GetDataPointById(dataPointId);
        if (dataPoint is null)
        {
            throw new DataPointNotFoundException(dataPointId);
        }

        return dataPoint;
    }

    public async Task<DataPointEntry[]> GetDataPointEntries(string organizationId, string dataPointKey,
        DateTime periodDateTime)
    {
        await ValidateOrganization(organizationId);
        var dataPointEntries =
            await _dataPointEntryRepository.GetDataPointEntries(organizationId, dataPointKey, periodDateTime);
        return dataPointEntries.ToArray();
    }

    public async Task<double> CalculateChangeOverTime(DataPoint dataPoint, DateTime periodDateTime)
    {
        var dataPointEntry = await _dataPointEntryRepository.GetDataPointEntryFromPreviousPeriod(
            dataPoint.OrganizationId,
            dataPoint.DataPointKey, periodDateTime);
        if (dataPointEntry is null)
        {
            return 0;
        }

        var previousValue = dataPoint.CalculateEntryValueWithFormula(dataPointEntry.Value);
        return dataPoint.CalculateChangeOverTime(previousValue);
    }

    public async Task AddHistoricDataPointEntries(DataPointEntry[] dataPointEntries, string organizationId)
    {
        foreach (var dataPointEntry in dataPointEntries)
        {
            // Add missing values to data point entry 
            dataPointEntry.Id = IdGenerator.GenerateId();
            dataPointEntry.OrganizationId = organizationId;
            var dataPoints =
                await _dataPointRepository.FindDataPointsByKey(dataPointEntry.DataPointKey,
                    dataPointEntry.OrganizationId);

            if (dataPoints.Length == 0)
            {
                await CreateDataPoint(organizationId, dataPointEntry.DataPointKey, dataPointEntry.Value);
            }
        }

        await _dataPointEntryRepository.AddDataPointEntries(dataPointEntries);
    }

    public async Task<DataPoint[]> GetAllDataPoints(string organizationId)
    {
        await ValidateOrganization(organizationId);
        return await _dataPointRepository.GetAllDatapointForOrganization(organizationId);
    }

    public async Task AddDataPointEntry(string dataPointKey, double value, string organizationId)
    {
        var dataPointEntry = CreateDataPointEntry(dataPointKey, value, organizationId);
        await _dataPointEntryRepository.AddDataPointEntry(dataPointEntry);
        await UpdateDataPointsWithMatchingKeys(dataPointEntry);
    }

    public async Task AddDataPointEntries(DataPointEntry[] dataPointEntries, string organizationId)
    {
        var result = new List<DataPointEntry>();
        foreach (var entry in dataPointEntries)
        {
            var dataPointEntry = CreateDataPointEntry(entry.DataPointKey, entry.Value, organizationId);

            await UpdateDataPointsWithMatchingKeys(dataPointEntry);
            result.Add(dataPointEntry);
        }

        await _dataPointEntryRepository.AddDataPointEntries(result.ToArray());
    }

    public async Task DeleteDataPoint(string dataPointId, bool forceDelete)
    {
        var dataPoint = await _dataPointRepository.GetDataPointById(dataPointId);
        if (dataPoint is null)
        {
            throw new DataPointNotFoundException(dataPointId);
        }

        var dataPointsWithMatchingKey =
            await _dataPointRepository.FindDataPointsByKey(dataPoint.DataPointKey, dataPoint.OrganizationId);

        if (dataPointsWithMatchingKey.Length > 1)
        {
            await _dataPointRepository.DeleteDataPointById(dataPoint.Id);
            return;
        }
        
        if (!forceDelete && dataPointsWithMatchingKey.Length == 1)
        {
            throw new DeleteDataPointWarningException(dataPoint.DataPointKey);
        }
        
        await _dataPointEntryRepository.DeleteAllEntriesByDataPointKey(dataPoint.DataPointKey, dataPoint.OrganizationId);
        await _dataPointRepository.DeleteDataPointByKey(dataPoint.DataPointKey, dataPoint.OrganizationId);
    }

    private DataPointEntry CreateDataPointEntry(string dataPointKey, double value, string organizationId)
    {
        var dataPointEntry = new DataPointEntry
        {
            Id = IdGenerator.GenerateId(),
            DataPointKey = dataPointKey,
            OrganizationId = organizationId,
            Value = value,
            Time = DateTime.UtcNow,
        };
        return dataPointEntry;
    }

    private async Task UpdateDataPointsWithMatchingKeys(DataPointEntry dataPointEntry)
    {
        var dataPoints =
            await _dataPointRepository.FindDataPointsByKey(dataPointEntry.DataPointKey, dataPointEntry.OrganizationId);

        //Create new Data point with matching dataPointKey if none exist, else update latest value
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
    }

    public async Task UpdateDataPoint(DataPoint dataPoint)
    {
        await ValidateOrganization(dataPoint.OrganizationId);

        // Update latest value to match the newest formula
        await UpdateDataPointLatestValue(dataPoint);
        await _dataPointRepository.UpdateDataPoint(dataPoint);
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
        var organizationExists = await _organizationDomainService.OrganizationExists(organizationId);
        if (!organizationExists)
        {
            throw new OrganizationNotFoundException(organizationId);
        }
    }
}