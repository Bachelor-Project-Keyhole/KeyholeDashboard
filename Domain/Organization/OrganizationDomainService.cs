using Domain.Exceptions;
using Domain.Organization.OrganizationUserInvite;
using Domain.RepositoryInterfaces;

namespace Domain.Organization;

public class OrganizationDomainService : IOrganizationDomainService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IOrganizationUserInviteRepository _organizationUserInviteRepository;

    public OrganizationDomainService(
        IOrganizationRepository organizationRepository,
        IOrganizationUserInviteRepository organizationUserInviteRepository)
    {
        _organizationRepository = organizationRepository;
        _organizationUserInviteRepository = organizationUserInviteRepository;
    }

    public async Task<bool> OrganizationExists(string organizationId)
    {
        return await _organizationRepository.OrganizationExists(organizationId);
    }

    public async Task<Organization> GetOrganizationById(string organizationId)
    {
        var organization = await _organizationRepository.GetOrganizationById(organizationId);
        if (organization == null)
            throw new OrganizationNotFoundException($"Organization with given id: {organizationId} was not found");
        return organization;
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

    public async Task Update(Organization organization)
    {
        await _organizationRepository.UpdateOrganization(organization);
    }

    public async Task<List<OrganizationUserInvites>> GetAllInvitesByOrganizationId(string organizationId)
    {
        var invitations = await _organizationUserInviteRepository.GetAllInvitesByOrganizationId(organizationId);
        if (invitations == null)
            throw new InvitationNotFound($"No invitations were found by given organization Id: {organizationId}");
        return invitations;
    }

    public async Task<OrganizationUserInvites> GetInvitationById(string invitationId,string organizationId)
    {
        var invitation = await _organizationUserInviteRepository.GetInvitationById(invitationId, organizationId);
        if (invitation == null)
            throw new InvitationNotFound($"Invitation by Id: {invitationId} was not found");
        return invitation;
    }

    public async Task<OrganizationUserInvites> GetInvitationByEmail(string email, string organizationId)
    {
        var invitation = await _organizationUserInviteRepository.GetInvitationByEmail(email, organizationId);
        if (invitation == null)
            throw new InvitationNotFound($"Invitation by email: {email} was not found"); 
        return invitation;
    }

    public async Task<OrganizationUserInvites> GetInvitationByToken(string token)
    {
        var invitation = await _organizationUserInviteRepository.GetInvitationByToken(token);
        if (invitation == null)
            throw new InvitationTokenException($"Invitation token: {token} was not found ");
        if(invitation.TokenExpirationTime < DateTime.UtcNow )
            throw new InvitationTokenException($"Invitation token: {token} has already expired");
        if(invitation.HasAccepted)
            throw new InvitationTokenException($"Invitation token: {token} was already used ");

        return invitation;
    }

    public async Task InsertInviteUser(OrganizationUserInvites invite)
    {
        await _organizationUserInviteRepository.InsertInviteUser(invite);
    }

    public async Task UpdateUserInvite(OrganizationUserInvites invite)
    {
        await _organizationUserInviteRepository.UpdateUserInvite(invite);
    }

    public async Task RemoveInvitationByToken(string token)
    {
        await _organizationUserInviteRepository.RemoveInvitationByToken(token);
    }

    public async Task RemoveInvitationById(string invitationId, string organizationId)
    {
        var invitation = await _organizationUserInviteRepository.GetInvitationById(invitationId, organizationId);
        if (invitation == null) // To show that user cannot be deleted
            throw new InvitationNotFound($"invitation with Id: {invitationId} was not found");
        await _organizationUserInviteRepository.RemoveInvitationById(invitationId);
    }
}