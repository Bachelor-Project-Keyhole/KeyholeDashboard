using Domain.User;

namespace Domain.Organization;

public class OrganizationUserInvites
{
    public string? Id { get; set; }
    public string OrganizationId { get; set; }
    public string Token { get; set; }
    public string ReceiverEmail { get; set; }
    public List<UserAccessLevel> AccessLevels { get; set; }
    public InviteStatus InviteStatus{ get; set; }
    public DateTime TokenExpirationTime { get; set; }
    public string InvitedByUserId { get; set; }
}