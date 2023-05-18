using Application.Organization.Model;

namespace Application.Organization;

public interface IOrganizationService
{
    Task<(string, string)> InviteUser(OrganizationUserInviteRequest request);
    Task<Domain.Organization.OrganizationUserInvites> TokenValidity(string token);
    Task<AllUsersOfOrganizationResponse> GetAllUsers(string id);
}