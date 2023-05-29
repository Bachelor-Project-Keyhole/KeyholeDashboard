namespace Domain.TwoFactor;

public interface ITwoFactorDomainService
{
    Task<TwoFactor> GetByToken(string token);
    Task DeleteById(string tokenId);
    Task DeleteByToken(string token);
}