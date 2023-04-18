namespace Repository.TwoFactor.TwoFactorReadModel;

public interface ITwoFactorReadModel
{
    Task<TwoFactorPersistence?> GetByIdentifier(string identifier);
}