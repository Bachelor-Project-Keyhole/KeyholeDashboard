using AutoMapper;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Repository.Dashboard;

public class DashboardRepository : MongoRepository<DashboardPersistenceModel>, IDashboardRepository
{
    private readonly IMapper _mapper;
    
    public DashboardRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task<Domain.Dashboard.Dashboard?> GetDashboardById(string dashboardId)
    {
        var dashboard = await FindByIdAsync(dashboardId);
        return _mapper.Map<Domain.Dashboard.Dashboard>(dashboard);
    }

    public async Task<List<Domain.Dashboard.Dashboard>?> GetAllDashboards(string organizationId)
    {
        var dashboards = await FilterByAsync(x => x.OrganizationId == organizationId);
        return _mapper.Map<List<Domain.Dashboard.Dashboard>>(dashboards);
    }

    public async Task Insert(Domain.Dashboard.Dashboard dashboard)
    {
        var dashboardPersistence = _mapper.Map<DashboardPersistenceModel>(dashboard);
        await InsertOneAsync(dashboardPersistence);
    }

    public async Task Update(Domain.Dashboard.Dashboard dashboard)
    {
        var dashboardPersistence = _mapper.Map<DashboardPersistenceModel>(dashboard);
        await ReplaceOneAsync(dashboardPersistence);
    }

    public async Task Delete(string id)
    {
        await DeleteOneAsync(x => x.Id == ObjectId.Parse(id));
    }
}