using Domain.Organization.OrganizationUserInvite;

namespace Domain.RepositoryInterfaces;

public interface IOrganizationUserInviteRepository
{
    Task InsertInviteUser(OrganizationUserInvites insert);
    Task UpdateUserInvite(OrganizationUserInvites insert);
    Task<OrganizationUserInvites?> GetByToken(string token);
}