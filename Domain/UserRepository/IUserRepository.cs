namespace Domain.UserRepository;

public interface IUserRepository
{
    Task<Domain.DomainEntities.User?> GetUserById(string id);
    Task<Domain.DomainEntities.User?> GetUserByEmail(string email);
    Task<Domain.DomainEntities.User?> GetByRefreshToken(string token);
    Task UpdateUser(Domain.DomainEntities.User user);
}