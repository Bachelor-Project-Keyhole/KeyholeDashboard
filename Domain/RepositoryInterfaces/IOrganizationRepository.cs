namespace Domain.RepositoryInterfaces;

public interface IOrganizationRepository
{
    Task Insert(Organization.Organization organization);
    Task<bool> OrganizationExists(string organizationId);
}