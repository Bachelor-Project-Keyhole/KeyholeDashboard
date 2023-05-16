namespace Domain.RepositoryInterfaces;

public interface IOrganizationRepository
{
    Task<bool> OrganizationExists(string organizationId);
    Task<Organization.Organization?> GetOrganizationById(string id);
    Task Insert(Organization.Organization organizationToInsert);
}