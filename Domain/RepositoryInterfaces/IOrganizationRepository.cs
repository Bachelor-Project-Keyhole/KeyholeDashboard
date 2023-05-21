namespace Domain.RepositoryInterfaces;

public interface IOrganizationRepository
{
    Task<bool> OrganizationExists(string organizationId);
    Task<Domain.Organization.Organization?> GetOrganizationById(string id);
    Task Insert(Organization.Organization organizationToInsert);
    Task UpdateOrganization(Organization.Organization organization);

}