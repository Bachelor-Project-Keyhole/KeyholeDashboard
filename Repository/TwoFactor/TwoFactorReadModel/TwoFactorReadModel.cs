using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Repository.TwoFactor.TwoFactorReadModel;

public class TwoFactorReadModel : ITwoFactorReadModel
{
    private readonly IMongoDatabase _database;

    public TwoFactorReadModel(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<TwoFactorPersistence?> GetByIdentifier(string identifier)
    {
        return await _database.GetCollection<TwoFactorPersistence>(nameof(Domain.DomainEntities.TwoFactor))
            .AsQueryable().Where(x => x.Identifier == identifier).SingleOrDefaultAsync();
    }
}