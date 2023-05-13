namespace Domain.RepositoryInterfaces;

public interface ITwoFactorRepository
{
    Task<TwoFactor.TwoFactor?> GetByIdentifier(string email);

    Task Delete(string id);
    Task Insert(TwoFactor.TwoFactor twoFactor);
}