using MongoDB.Driver;

namespace Repository.User.UserPersistence.WriteModel;

public class UserWriteModel : IUserWriteModel
{
    private readonly IMongoDatabase _database;

    public UserWriteModel(IMongoDatabase database)
    {
        _database = database;
    }
}