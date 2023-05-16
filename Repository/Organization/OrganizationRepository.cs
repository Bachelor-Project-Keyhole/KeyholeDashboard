using AutoMapper;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;

namespace Repository.Organization;

public class OrganizationRepository : MongoRepository<OrganizationPersistenceModel>, IOrganizationRepository
{
    private readonly IMapper _mapper;
    public OrganizationRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task<Domain.Organization.Organization?> GetOrganizationById(string id)
    {
        var organizationPersistence = await FindByIdAsync(id);
        return _mapper.Map<Domain.Organization.Organization>(organizationPersistence);
    }

    public async Task Insert(Domain.Organization.Organization organization)
    {
        var organizationEntity = _mapper.Map<OrganizationPersistenceModel>(organization);
        await InsertOneAsync(organizationEntity);
    }
    
    public async Task<bool> OrganizationExists(string organizationId)
    {
        var result = await FindByIdAsync(organizationId);
        return result is not null;

    }
}