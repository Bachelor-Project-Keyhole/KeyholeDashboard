namespace Domain.RepositoryInterfaces;

public interface IOrganizationUserInviteRepository
{
    Task InsertInviteUser(Organization.OrganizationUserInvites insert);
    Task UpdateUserInvite(Organization.OrganizationUserInvites insert);
    Task<Domain.Organization.OrganizationUserInvites?> GetByToken(string token);
}