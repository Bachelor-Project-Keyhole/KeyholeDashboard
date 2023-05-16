namespace Domain.RepositoryInterfaces;

public interface IOrganizationUserInviteRepository
{
    Task InsertInviteUser(Organization.OrganizationUserInvites insert);
}