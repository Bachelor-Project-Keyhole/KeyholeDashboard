using Domain.User;

namespace Application.User.Model;

public class UserChangeAccessResponse
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public List<UserAccessLevel> AccessLevels { get; set; }
}