using AutoMapper;
using Domain.Datapoint;
using Microsoft.Extensions.Options;

namespace Repository.Datapoint;

public class DataPointRepository : MongoRepository<DataPointEntity>, IDataPointRepository
{
    private readonly IMapper _mapper;
    
    public DataPointRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task<DataPoint[]> GetAllDatapointForOrganization(string organizationId)
    {
        var datapointEntities =
            await FilterByAsync(dataPoint => dataPoint.OrganizationId == organizationId);
        return _mapper.Map<DataPoint[]>(datapointEntities);
    }

    public async Task CreateDataPoint(DataPoint dataPoint)
    {
        var dataPointEntity = _mapper.Map<DataPointEntity>(dataPoint);
        await InsertOneAsync(dataPointEntity);
    }

    public async Task UpdateDataPoint(DataPoint dataPoint)
    {
        var dataPointEntity = _mapper.Map<DataPointEntity>(dataPoint);
        await ReplaceOneAsync(dataPointEntity);
    }

    public async Task<DataPoint?> FindDataPointByKey(string key, string organizationId)
    {
        var dataPointEntity = 
            await FindOneAsync(dp => dp.OrganizationId == organizationId && dp.DataPointKey == key);
        return _mapper.Map<DataPoint>(dataPointEntity);
    }
}