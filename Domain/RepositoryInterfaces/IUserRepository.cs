using Domain.DomainEntities;

namespace Domain.RepositoryInterfaces;

public interface IUserRepository
{
    Task<User?> GetUserById(string id);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetByRefreshToken(string token);
    Task UpdateUser(User user);
    Task CreateUser(User user);
}