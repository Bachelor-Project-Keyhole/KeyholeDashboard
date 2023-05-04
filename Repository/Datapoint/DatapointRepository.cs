using AutoMapper;
using Domain.Datapoint;
using Microsoft.Extensions.Options;

namespace Repository.Datapoint;

public class DatapointRepository : MongoRepository<DatapointEntity>, IDatapointRepository
{
    private readonly IMapper _mapper;
    
    public DatapointRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task<DataPoint[]> GetAllDatapointForOrganization(string organizationId)
    {
        var datapointEntities = 
            AsQueryable()
                .Where(datapoint => datapoint.OrganizationId == organizationId)
                .ToArray();
        return _mapper.Map<DataPoint[]>(datapointEntities);
    }
}