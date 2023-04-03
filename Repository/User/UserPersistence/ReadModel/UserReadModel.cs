using AutoMapper;
using MongoDB.Driver;

namespace Repository.User.UserPersistence.ReadModel;

public class UserReadModel : IUserReadModel
{
    private readonly IMongoDatabase _database;

    public UserReadModel(IMongoDatabase database)
    {
        _database = database;
    }
}