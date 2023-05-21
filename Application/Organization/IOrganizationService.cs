using Application.Organization.Model;
using Domain.Organization.OrganizationUserInvite;

namespace Application.Organization;

public interface IOrganizationService
{
    Task<(string, string)> InviteUser(OrganizationUserInviteRequest request);
    Task<OrganizationUserInvites> TokenValidity(string token);
    Task<OrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request);
}