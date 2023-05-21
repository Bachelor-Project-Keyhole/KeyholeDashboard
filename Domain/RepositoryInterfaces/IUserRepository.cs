namespace Domain.RepositoryInterfaces;

public interface IUserRepository
{
    Task<User.User?> GetUserById(string id);
    Task<User.User?> GetUserByEmail(string email);
    Task<List<Domain.User.User>?> GetAllUsersByOrganizationId(string organizationId);
    Task<User.User?> GetByRefreshToken(string token);
    Task UpdateUser(User.User user);
    Task CreateUser(User.User user);
    Task RemoveUserById(string userId);
}