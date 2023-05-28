using Contracts.v1.Organization;

namespace Application.Organization;

public interface IOrganizationApplicationService
{
    Task<OrganizationDetailedResponse> GetOrganizationById(string organizationId);
    Task<(string, string)> InviteUser(OrganizationUserInviteRequest request);
    Task<Domain.Organization.OrganizationUserInvite.OrganizationUserInvites> TokenValidity(string token);
    Task<OrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request);

    Task<Domain.Organization.OrganizationUserInvite.OrganizationUserInvites> GetInvitationById(string invitationId, string organizationId);
    Task<Domain.Organization.OrganizationUserInvite.OrganizationUserInvites> GetInvitationByEmail(string email, string organizationId);
    Task<List<Domain.Organization.OrganizationUserInvite.OrganizationUserInvites>> GetInvitationByOrganizationId(string organizationId);
    Task RemoveInvitationById(string invitationId, string organizationId);
}