using AutoMapper;
using Domain.Organization;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;

namespace Repository.Organization;

public class OrganizationRepository : MongoRepository<OrganizationEntity>, IOrganizationRepository
{
    private readonly IMapper _mapper;
    public OrganizationRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task Insert(Domain.Organization.Organization organization)
    {
        var organizationEntity = _mapper.Map<OrganizationEntity>(organization);
        await InsertOneAsync(organizationEntity);
    }

    public async Task Insert(TempOrganization organization)
    {
        var organizationEntity = _mapper.Map<OrganizationEntity>(organization);
        await InsertOneAsync(organizationEntity);
    }

    public async Task<bool> OrganizationExists(string organizationId)
    {
        var result = await FindByIdAsync(organizationId);
        return result is not null;

    }
}