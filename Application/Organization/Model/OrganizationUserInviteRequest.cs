using Domain.User;

namespace Application.Organization.Model;

public class OrganizationUserInviteRequest
{
    public string OrganizationId { get; set; }
    public string UserId { get; set; }
    public string ReceiverEmailAddress { get; set; }
    public List<UserAccessLevel> AccessLevels { get; set; }
    public string? Message { get; set; } // if this is not null, it will overhaul default message
}