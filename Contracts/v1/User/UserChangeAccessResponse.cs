using Domain.User;

namespace Contracts.v1.User;

public class UserChangeAccessResponse
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public List<UserAccessLevel> AccessLevels { get; set; }
}