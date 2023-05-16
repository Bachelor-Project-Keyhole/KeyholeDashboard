using Application.Organization.Model;
using Domain.Exceptions;
using Domain.RepositoryInterfaces;

namespace Application.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;

    public OrganizationService(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }
    
    public async Task InviteUser(OrganizationUserInviteRequest request)
    {
        var organization = await _organizationRepository.GetOrganizationById(request.OrganizationId);
        if (organization == null)
            throw new OrganizationNotFoundException($"Organization with given id: {request.OrganizationId} was not found");
        
        if(organization.)
    }
}