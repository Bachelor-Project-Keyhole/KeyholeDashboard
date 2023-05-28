using Contracts.v1.Organization;
using Domain.Exceptions;
using Domain.Organization;
using Domain.Organization.OrganizationUserInvite;
using Domain.User;
using Microsoft.Extensions.Options;

namespace Application.Organization;

public class OrganizationApplicationService : IOrganizationApplicationService
{
    private readonly IOrganizationDomainService _organizationDomainService;
    private readonly InvitationBaseRoute _invitationBaseRoute;

    public OrganizationApplicationService(
        IOrganizationDomainService organizationDomainService,
        IOptions<InvitationBaseRoute> invitationBaseRoute)
    {
        _organizationDomainService = organizationDomainService;
        _invitationBaseRoute = invitationBaseRoute.Value;
    }

    public async Task<OrganizationDetailedResponse> GetOrganizationById(string organizationId)
    {
        var organization = await _organizationDomainService.GetOrganizationById(organizationId);
        
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
        var organization = await _organizationDomainService.GetOrganizationById(request.OrganizationId);
        
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
            HasAccepted = false,
            TokenExpirationTime = DateTime.UtcNow.AddDays(2),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(5),
            AccessLevels = accessLevelsToInsert
        };
        
        // Delete outdated Invitations
        var invitations = await _organizationDomainService.GetAllInvitesByOrganizationId(organization.Id);

        if (invitations.Count > 0) // TODO: ADD test
            await RemoveAllOutdatedInvitations(invitations);
        
        await _organizationDomainService.InsertInviteUser(insert);
        
        var link = $"{_invitationBaseRoute.Link}/{alphaNumericToken}";
        return (link, organization.OrganizationName);
    }

    public async Task<OrganizationUserInvites> TokenValidity(string token)
    {
        var invitation = await _organizationDomainService.GetInvitationByToken(token);
        
        invitation.HasAccepted = true;
        await _organizationDomainService.UpdateUserInvite(invitation);
        return invitation;
    }

    public async Task<OrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request)
    {
        var organization = await _organizationDomainService.GetOrganizationById(request.OrganizationId);
        organization.OrganizationName = request.OrganizationName;
        organization.ModificationDate = DateTime.UtcNow;
        await _organizationDomainService.Update(organization);
        return new OrganizationResponse
        {
            OrganizationId = organization.Id,
            OrganizationOwnerId = organization.OrganizationOwnerId,
            OrganizationName = organization.OrganizationName,
            CreationDate = organization.CreationDate.ToLocalTime(),
            ModificationDate = organization.ModificationDate.ToLocalTime()
        };
    }

    public async Task<OrganizationUserInvites> GetInvitationById(string invitationId, string organizationId)
    {
        await _organizationDomainService.GetOrganizationById(organizationId); // To validate organization
        var invitation = await _organizationDomainService.GetInvitationById(invitationId, organizationId);
        return invitation;
    }

    public async Task<OrganizationUserInvites> GetInvitationByEmail(string email, string organizationId)
    {
        await _organizationDomainService.GetOrganizationById(organizationId); // To validate organization
        return await _organizationDomainService.GetInvitationByEmail(email, organizationId);
    }

    public async Task<List<OrganizationUserInvites>> GetInvitationByOrganizationId(string organizationId)
    {
       return await _organizationDomainService.GetAllInvitesByOrganizationId(organizationId);
    }

    public async Task RemoveInvitationById(string invitationId, string organizationId)
    {
        await _organizationDomainService.GetOrganizationById(organizationId); // To validate organization
        await _organizationDomainService.RemoveInvitationById(invitationId, organizationId);
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
        var validInvites = invites.Where(x => x.TokenExpirationTime > DateTime.UtcNow).ToList();
        invites.RemoveAll(x => validInvites.Contains(x));
        foreach (var invite in invites.Where(invite => invite.RemoveFromDbDate < DateTime.UtcNow))
        {
            await _organizationDomainService.RemoveInvitationByToken(invite.Token);
        }
    }
}
