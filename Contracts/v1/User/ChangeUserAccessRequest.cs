namespace Contracts.v1.User;

public class ChangeUserAccessRequest
{
    public string UserId { get; set; }
    public string SetAccessLevel { get; set; }
}