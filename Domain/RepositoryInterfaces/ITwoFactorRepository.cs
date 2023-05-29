namespace Domain.RepositoryInterfaces;

public interface ITwoFactorRepository
{
    Task<TwoFactor.TwoFactor?> GetByIdentifier(string email);
    Task<TwoFactor.TwoFactor?> GetByToken(string token);

    Task DeleteById(string tokenId);
    Task DeleteByToken(string token);
    Task Insert(TwoFactor.TwoFactor twoFactor);
}