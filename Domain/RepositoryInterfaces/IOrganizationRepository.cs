using Domain.Organization;

namespace Domain.RepositoryInterfaces;

public interface IOrganizationRepository
{
    Task Insert(TempOrganization organization);
    Task<bool> OrganizationExists(string organizationId);
}