using Application.Organization.Model;
using Domain.Exceptions;
using Domain.Organization;
using Domain.Organization.OrganizationUserInvite;
using Domain.RepositoryInterfaces;

namespace Application.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IOrganizationUserInviteRepository _organizationUserInvite;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        IOrganizationUserInviteRepository organizationUserInvite)
    {
        _organizationRepository = organizationRepository;
        _organizationUserInvite = organizationUserInvite;
    }
    
    public async Task<(string, string)> InviteUser(OrganizationUserInviteRequest request)
    {
        var organization = await _organizationRepository.GetOrganizationById(request.OrganizationId);
        if (organization == null)
            throw new OrganizationNotFoundException($"Organization with given id: {request.OrganizationId} was not found");

        var alphaNumericToken = GenerateAlphaNumeric();

        var insert = new OrganizationUserInvites
        {
            OrganizationId = request.OrganizationId,
            Token = alphaNumericToken,
            ReceiverEmail = request.ReceiverEmailAddress,
            hasAccepted = false,
            TokenExpirationTime = DateTime.UtcNow.AddDays(2),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(5),
            AccessLevels = request.AccessLevels,
            InvitedByUserId = request.UserId
        };

        await _organizationUserInvite.InsertInviteUser(insert);

        var link = $"https://keyholedashboard.azurewebsites.net/organization/register/{alphaNumericToken}";
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
            CreationDate = organization.CreationDate,
            ModificationDate = organization.ModificationDate
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
}
