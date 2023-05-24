namespace Domain.Organization;

public interface IOrganizationDomainService
{
    Task<bool> OrganizationExists(string organizationId);
    Task<Organization> GetOrganizationByApiKey(string apiKey);
}