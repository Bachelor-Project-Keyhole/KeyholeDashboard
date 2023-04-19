using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Repository.User.UserPersistence;

namespace Repository.User.UserReadModel;

public class UserReadModel : IUserReadModel
{
    private readonly IMongoDatabase _database;

    public UserReadModel(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<UserPersistenceModel?> GetUserById(ObjectId id)
    {
        return await _database.GetCollection<UserPersistenceModel>(nameof(Domain.DomainEntities.User)).AsQueryable()
            .Where(x => x.Id == id).SingleOrDefaultAsync();
    }

    public async Task<UserPersistenceModel?> GetUserByEmail(string email)
    {
        return await _database.GetCollection<UserPersistenceModel>(nameof(Domain.DomainEntities.User)).AsQueryable()
            .Where(x => x.Email == email).SingleOrDefaultAsync();
    }

    public async Task<UserPersistenceModel?> GetByRefreshToken(string token)
    {
        return await _database.GetCollection<UserPersistenceModel>(nameof(Domain.DomainEntities.User)).AsQueryable()
            .Where(x => x.RefreshTokens!.Any(y => y.Token == token)).SingleOrDefaultAsync();
    }
}