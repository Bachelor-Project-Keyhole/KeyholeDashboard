using Application.Organization.Model;

namespace Application.Organization;

public interface IOrganizationService
{
    Task<(string, string)> InviteUser(OrganizationUserInviteRequest request);
}