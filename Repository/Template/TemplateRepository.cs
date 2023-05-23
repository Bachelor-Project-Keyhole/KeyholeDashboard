using AutoMapper;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Repository.Template;

public class TemplateRepository : MongoRepository<TemplatePersistenceModel>, ITemplateRepository
{
    private readonly IMapper _mapper;
    
    public TemplateRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task<Domain.Template.Template?> GetById(string id)
    {
        var template = await FindOneAsync(x => x.Id == ObjectId.Parse(id));
        return _mapper.Map<Domain.Template.Template>(template);
    }

    public async Task<List<Domain.Template.Template>?> GetAllByDashboardId(string dashboardId)
    {
        var templates = await FilterByAsync(x => x.DashboardId == dashboardId);
        return _mapper.Map<List<Domain.Template.Template>>(templates);
    }

    public async Task Insert(Domain.Template.Template template)
    {
        var persistenceModel = _mapper.Map<TemplatePersistenceModel>(template);
        await InsertOneAsync(persistenceModel);
    }

    public async Task Update(Domain.Template.Template template)
    {
        var persistenceModel = _mapper.Map<TemplatePersistenceModel>(template);
        await ReplaceOneAsync(persistenceModel);
    }

    public async Task DeleteTemplate(string id)
    {
        await DeleteOneAsync(x => x.Id == ObjectId.Parse(id));
    }

    public async Task RemoveAllTemplatesWithDashboardId(string dashboardId)
    {
        await DeleteManyAsync(x => x.DashboardId == dashboardId);
    }
}