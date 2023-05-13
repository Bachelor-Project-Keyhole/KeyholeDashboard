using Domain.User;

namespace Application.User.Model;

public class ChangeUserAccessRequest
{
    public string UserId { get; set; }
    public string AdminUserId { get; set; }
    public UserAccessLevel SetAccessLevel { get; set; }
}