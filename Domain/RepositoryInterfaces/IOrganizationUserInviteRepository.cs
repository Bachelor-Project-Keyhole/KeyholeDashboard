using Domain.Organization.OrganizationUserInvite;

namespace Domain.RepositoryInterfaces;

public interface IOrganizationUserInviteRepository
{
    Task<List<OrganizationUserInvites>?> GetAllInvitesByOrganizationId(string organizationId);
    Task<OrganizationUserInvites?> GetInvitationById(string invitationId, string organizationId);
    Task<OrganizationUserInvites?> GetInvitationByEmail(string email, string organizationId);
    Task InsertInviteUser(OrganizationUserInvites insert);
    Task UpdateUserInvite(OrganizationUserInvites insert);
    Task RemoveInvitationByToken(string token);
    Task<OrganizationUserInvites?> GetInvitationByToken(string token);
    Task RemoveInvitationById(string invitationId);
}