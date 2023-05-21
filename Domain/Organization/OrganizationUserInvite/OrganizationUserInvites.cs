using Domain.User;

namespace Domain.Organization.OrganizationUserInvite;

public class OrganizationUserInvites
{
    public string OrganizationId { get; set; }
    public string Token { get; set; }
    public string ReceiverEmail { get; set; }
    public List<UserAccessLevel> AccessLevels { get; set; }
    public bool hasAccepted{ get; set; }
    public DateTime TokenExpirationTime { get; set; }
    public DateTime RemoveFromDbDate { get; set; }
    public string InvitedByUserId { get; set; }
}