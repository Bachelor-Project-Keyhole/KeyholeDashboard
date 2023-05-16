using Application.Organization.Model;

namespace Application.Organization;

public interface IOrganizationService
{
    Task InviteUser(OrganizationUserInviteRequest request);
}