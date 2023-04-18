namespace Domain.RepositoryInterfaces;

public interface IOrganizationRepository
{
    Task Insert(Domain.DomainEntities.Organization organization);
}