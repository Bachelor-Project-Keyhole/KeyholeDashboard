using Domain.Organization;
using Domain.User;

namespace Repository.OrganizationUserInvite;
[BsonCollection("organization-invite")]
public class OrganizationUserInvitePersistence : Document
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