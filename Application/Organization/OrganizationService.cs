using Application.Organization.Model;
using Domain.Exceptions;
using Domain.Organization;
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

        // Not completely sure if we need this. 
        if (organization.Members?.FirstOrDefault(x => x.Id == request.UserId) == null)
            throw new UserInvalidActionException("User does not belong to the organization, that he is trying to invite to");
        var alphaNumericToken = GenerateAlphaNumeric();

        var insert = new Domain.Organization.OrganizationUserInvites
        {
            Id = null,
            OrganizationId = request.OrganizationId,
            Token = alphaNumericToken,
            ReceiverEmail = request.ReceiverEmailAddress,
            InviteStatus = InviteStatus.Pending,
            TokenExpirationTime = DateTime.UtcNow.AddDays(2),
            AccessLevels = request.AccessLevels,
            InvitedByUserId = request.UserId
        };

        await _organizationUserInvite.InsertInviteUser(insert);

        var link = " https://keyholedashboard.azurewebsites.net/organization/register/{alphaNumericToken}";
        return (link, organization.OrganizationName);
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
