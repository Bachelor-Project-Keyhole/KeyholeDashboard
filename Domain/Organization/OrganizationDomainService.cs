using Domain.Exceptions;
using Domain.RepositoryInterfaces;

namespace Domain.Organization;

public class OrganizationDomainService : IOrganizationDomainService
{
    private readonly IOrganizationRepository _organizationRepository;

    public OrganizationDomainService(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<bool> OrganizationExists(string organizationId)
    {
        return await _organizationRepository.OrganizationExists(organizationId);
    }

    public async Task<Organization> GetOrganizationByApiKey(string apiKey)
    {
        var organization = await _organizationRepository.GetOrganizationByApiKey(apiKey);
        if (organization is null)
        {
            throw new InvalidApiKeyException();
        }
        return organization;
    }
}