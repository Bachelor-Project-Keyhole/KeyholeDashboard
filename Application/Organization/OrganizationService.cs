using Contracts.v1.Organization;
using Domain.Exceptions;
using Domain.Organization.OrganizationUserInvite;
using Domain.RepositoryInterfaces;
using Domain.User;
using Microsoft.Extensions.Options;

namespace Application.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IOrganizationUserInviteRepository _organizationUserInvite;
    private readonly InvitationBaseRoute _invitationBaseRoute;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        IOrganizationUserInviteRepository organizationUserInvite,
        IOptions<InvitationBaseRoute> invitationBaseRoute)
    {
        _organizationRepository = organizationRepository;
        _organizationUserInvite = organizationUserInvite;
        _invitationBaseRoute = invitationBaseRoute.Value;
    }

    public async Task<OrganizationDetailedResponse> GetOrganizationById(string organizationId)
    {
        var organization = await _organizationRepository.GetOrganizationById(organizationId);
        if (organization == null)
            throw new OrganizationNotFoundException($"Organization with given id: {organizationId} was not found");

        return new OrganizationDetailedResponse
        {
            OrganizationId = organization.Id,
            OrganizationOwnerId = organization.OrganizationOwnerId,
            OrganizationName = organization.OrganizationName,
            ApiKey = organization.ApiKey,
            CreationDate = organization.CreationDate.ToLocalTime(),
            ModificationDate = organization.ModificationDate.ToLocalTime()
        };

    }

    public async Task<(string, string)> InviteUser(OrganizationUserInviteRequest request)
    {
        var organization = await _organizationRepository.GetOrganizationById(request.OrganizationId);
        if (organization == null)
            throw new OrganizationNotFoundException($"Organization with given id: {request.OrganizationId} was not found");
        
        var toEnum = Enum.TryParse(request.AccessLevel, out UserAccessLevel accessLevel);
        if(toEnum == false)
            throw new AccessLevelForbiddenException($"This access level does not exist: {request.AccessLevel}");


        var accessLevelsToInsert = accessLevel switch
        {
            UserAccessLevel.Admin => new List<UserAccessLevel> {UserAccessLevel.Admin, UserAccessLevel.Editor, UserAccessLevel.Viewer},
            UserAccessLevel.Editor => new List<UserAccessLevel> {UserAccessLevel.Editor, UserAccessLevel.Viewer},
            UserAccessLevel.Viewer => new List<UserAccessLevel> {UserAccessLevel.Viewer},
            _ => throw new AccessLevelForbiddenException("Access level does not exist")
        };

        var alphaNumericToken = GenerateAlphaNumeric();

        var insert = new OrganizationUserInvites
        {
            OrganizationId = request.OrganizationId,
            Token = alphaNumericToken,
            ReceiverEmail = request.ReceiverEmailAddress,
            hasAccepted = false,
            TokenExpirationTime = DateTime.UtcNow.AddDays(2),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(5),
            AccessLevels = accessLevelsToInsert
        };
        
        // Delete outdated Invitations
        var invitations = await _organizationUserInvite.GetAllInvitesByOrganizationId(organization.Id);

        if (invitations != null)
            await RemoveAllOutdatedInvitations(invitations);


        await _organizationUserInvite.InsertInviteUser(insert);
        
        var link = $"{_invitationBaseRoute.Link}/{alphaNumericToken}";
        return (link, organization.OrganizationName);
    }

    public async Task<OrganizationUserInvites> TokenValidity(string token)
    {
        var invitation = await _organizationUserInvite.GetByToken(token);
        if (invitation == null)
            throw new InvitationTokenException($"Invitation token: {token} was not found ");
        if(invitation.TokenExpirationTime < DateTime.UtcNow )
            throw new InvitationTokenException($"Invitation token: {token} has already expired");
        if(invitation.hasAccepted)
            throw new InvitationTokenException($"Invitation token: {token} was already used ");
        
       
        invitation.hasAccepted = true;
        await _organizationUserInvite.UpdateUserInvite(invitation);
        return invitation;
    }

    public async Task<OrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request)
    {
        var organization = await _organizationRepository.GetOrganizationById(request.OrganzationId);
        if (organization == null)
            throw new OrganizationNotFoundException($"Organization with id: {request.OrganzationId} was not found");

        organization.OrganizationName = request.OrganizationName;
        organization.ModificationDate = DateTime.UtcNow;
        await _organizationRepository.UpdateOrganization(organization);

        return new OrganizationResponse
        {
            OrganizationId = organization.Id,
            OrganizationOwnerId = organization.OrganizationOwnerId,
            OrganizationName = organization.OrganizationName,
            CreationDate = organization.CreationDate.ToLocalTime(),
            ModificationDate = organization.ModificationDate.ToLocalTime()
        };
    }

    private string GenerateAlphaNumeric()
    {
        var length = 8;
        Random random = new Random();
        string charCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => charCharacters[random.Next(charCharacters.Length)])
            .ToArray());
    }
    
    private async Task RemoveAllOutdatedInvitations(List<OrganizationUserInvites> invites)
    {
        var validInvites = new List<OrganizationUserInvites>();
        foreach (var invite in invites)
        {
            if (invite.RemoveFromDbDate < DateTime.UtcNow)
                await _organizationUserInvite.RemoveByToken(invite.Token);
        }
    }
}
