using Domain.Exceptions;
using Domain.RepositoryInterfaces;

namespace Domain.User;

public class UserDomainService : IUserDomainService
{
    private readonly IUserRepository _userRepository;

    public UserDomainService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<User> GetUserByEmail(string email)
    {
        var user = await _userRepository.GetUserByEmail(email);
        if (user == null)
            throw new UserNotFoundException("User by given email was not found");
        return user;
    }

    public async Task<User> GetByRefreshToken(string token)
    {
        var user = await _userRepository.GetByRefreshToken(token);
        if (user == null)
            throw new UserNotFoundException("User with given token was not found");
        return user;
    }

    public async Task<User> GetUserById(string userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
            throw new UserNotFoundException($"User with given email: {userId} was not found");
        return user;
    }

    public async Task<List<User>> GetAllUsersByOrganizationId(string organizationId)
    {
        var users = await _userRepository.GetAllUsersByOrganizationId(organizationId);
        if (users == null)
            throw new UserNotFoundException($"No users were found with organization Id: {organizationId}");
        return users;
    }

    public async Task CreateUser(User user)
    {
        await _userRepository.CreateUser(user);
    }

    public async Task UpdateUser(User user)
    {
        await _userRepository.UpdateUser(user);
    }

    public async Task RemoveUserById(string userId)
    {
        await _userRepository.RemoveUserById(userId);
    }
}