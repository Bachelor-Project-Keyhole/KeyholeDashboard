using AutoMapper;
using Domain.Datapoint;
using Microsoft.Extensions.Options;

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
}