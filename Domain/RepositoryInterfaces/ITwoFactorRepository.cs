namespace Domain.RepositoryInterfaces;

public interface ITwoFactorRepository
{
    Task<DomainEntities.TwoFactor?> GetByIdentifier(string email);

    Task Delete(string id);
    Task Insert(DomainEntities.TwoFactor twoFactor);
}