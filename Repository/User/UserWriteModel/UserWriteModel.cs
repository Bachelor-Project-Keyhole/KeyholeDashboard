using MongoDB.Driver;
using Repository.User.UserPersistence;

namespace Repository.User.UserWriteModel;

public class UserWriteModel : IUserWriteModel
{
    private readonly IMongoDatabase _database;

    public UserWriteModel(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task UpdateUser(UserPersistenceModel user)
    {
        await _database.GetCollection<UserPersistenceModel>(nameof(Domain.DomainEntities.User))
            .ReplaceOneAsync(x => x.Id == user.Id, user);
    }

    public async Task InsertUser(UserPersistenceModel user)
    {
        await _database.GetCollection<UserPersistenceModel>(nameof(Domain.DomainEntities.User)).InsertOneAsync(user);
    }
}