using Domain.Organization.OrganizationUserInvite;

namespace Domain.RepositoryInterfaces;

public interface IOrganizationUserInviteRepository
{
    Task<List<OrganizationUserInvites>?> GetAllInvitesByOrganizationId(string organizationId);
    Task InsertInviteUser(OrganizationUserInvites insert);
    Task UpdateUserInvite(OrganizationUserInvites insert);
    Task RemoveByToken(string token);
    Task<OrganizationUserInvites?> GetByToken(string token);
}