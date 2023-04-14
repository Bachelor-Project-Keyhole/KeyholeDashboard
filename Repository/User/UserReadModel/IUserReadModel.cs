using MongoDB.Bson;
using Repository.User.UserPersistence;

namespace Repository.User.UserReadModel;

public interface IUserReadModel
{
    Task<UserPersistenceModel?> GetUserById(ObjectId id);
    Task<UserPersistenceModel?> GetUserByEmail(string email);
    Task<UserPersistenceModel?> GetByRefreshToken(string token);
}