using Repository.User.UserPersistence;

namespace Application.User.UserService;

public interface IUserService
{
    Task<Domain.DomainEntities.User?> GetUserByEmail(string email);
    Task<Domain.DomainEntities.User?> GetByRefreshToken(string token);
    Task UpdateUser(Domain.DomainEntities.User user);

}