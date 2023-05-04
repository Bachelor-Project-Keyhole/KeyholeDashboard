using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;

namespace Repository.Organization;

public class OrganizationRepository : MongoRepository<OrganizationEntity>, IOrganizationRepository 
{
    public OrganizationRepository(IOptions<DatabaseOptions> dataBaseOptions) : base(dataBaseOptions)
    {
    }

    public Task Insert(Domain.Organization.Organization organization)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> OrganizationExists(string organizationId)
    {
        var result = await FindByIdAsync(organizationId);
        return result is not null;

    }
}