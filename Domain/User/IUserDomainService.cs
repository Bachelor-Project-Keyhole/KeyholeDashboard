namespace Domain.User;

public interface IUserDomainService
{
    Task<User> GetUserByEmail(string email);
    Task<User> GetByRefreshToken(string token);
    Task<User> GetUserById(string userId);
    Task<List<User>> GetAllUsersByOrganizationId(string organizationId);
    Task CreateUser(User user);
    Task UpdateUser(User user);
    Task RemoveUserById(string userId);
}