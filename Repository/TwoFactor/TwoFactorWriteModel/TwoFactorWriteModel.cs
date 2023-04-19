using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository.TwoFactor.TwoFactorWriteModel;

public class TwoFactorWriteModel : ITwoFactorWriteModel
{
    private readonly IMongoDatabase _database;

    public TwoFactorWriteModel(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task Insert(TwoFactorPersistence twoFactor)
    {
        await _database.GetCollection<TwoFactorPersistence>(nameof(Domain.DomainEntities.TwoFactor))
            .InsertOneAsync(twoFactor);
    }

    public async Task Delete(ObjectId id)
    {
        await _database.GetCollection<TwoFactorPersistence>(nameof(Domain.DomainEntities.TwoFactor))
            .DeleteOneAsync(x => x.Id == id);
    }
}