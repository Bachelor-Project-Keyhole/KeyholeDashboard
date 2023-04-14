using Domain.UserRepository;
using Repository.User.UserPersistence;
using Repository.User.UserRepository;

namespace Application.User.UserService;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<Domain.DomainEntities.User?> GetUserByEmail(string email)
    {
        return await _userRepository.GetUserByEmail(email);
    }

    public async Task<Domain.DomainEntities.User?> GetByRefreshToken(string token)
    {
        return await _userRepository.GetByRefreshToken(token);
    }

    public async Task UpdateUser(Domain.DomainEntities.User user)
    {
        await _userRepository.UpdateUser(user);
    }
}