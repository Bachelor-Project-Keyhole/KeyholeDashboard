using AutoMapper;
using Domain.Datapoint;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Repository.Datapoint;

public class DataPointEntryRepository : MongoRepository<DataPointEntryEntity>, IDataPointEntryRepository
{
    private readonly IMapper _mapper;

    public DataPointEntryRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task AddDataPointEntry(DataPointEntry dataPointEntry)
    {
        var dataPointEntryEntity = _mapper.Map<DataPointEntryEntity>(dataPointEntry);
        await InsertOneAsync(dataPointEntryEntity);
    }

    public async Task<DataPointEntry[]> GetAllDataPointEntries(string organizationId, string key)
    {
        var result =
            await FilterByAsync(d => d.OrganizationId == organizationId && d.DataPointKey == key);
        return _mapper.Map<DataPointEntry[]>(result);
    }

    public async Task<DataPointEntry?> GetLatestDataPointEntry(string organizationId, string dataPointKey)
    {
        var latestDataPointEntry = await Collection
            .Find(dpe => dpe.OrganizationId == organizationId && dpe.DataPointKey == dataPointKey)
            .SortByDescending(entry => entry.Time)
            .FirstOrDefaultAsync();
        return _mapper.Map<DataPointEntry>(latestDataPointEntry);
    }

    public async Task<IEnumerable<DataPointEntry>> GetDataPointEntries(string organizationId, string dataPointKey,
        int timeSpanInDays)
    {
        var startDate = DateTime.Now.AddDays(-timeSpanInDays);
        var filter = Builders<DataPointEntryEntity>.Filter.And(
            Builders<DataPointEntryEntity>.Filter.Eq("OrganizationId", organizationId),
            Builders<DataPointEntryEntity>.Filter.Eq("DataPointKey", dataPointKey),
            Builders<DataPointEntryEntity>.Filter.Gte("Time", startDate)
        );

        var sort = Builders<DataPointEntryEntity>.Sort.Ascending("Time");

        var result = await Collection.Find(filter)
            .Sort(sort)
            .ToListAsync();

        return _mapper.Map<DataPointEntry[]>(result);
    }
}