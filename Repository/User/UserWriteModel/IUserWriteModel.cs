using Repository.User.UserPersistence;

namespace Repository.User.UserWriteModel;

public interface IUserWriteModel
{
    Task UpdateUser(UserPersistenceModel user);
    Task InsertUser(UserPersistenceModel user);
}