namespace Contracts.v1.Organization;

public class PendingUserInvitationResponse
{
    public string Id { get; set; }
    public string OrganizationId { get; set; }
    public string Token { get; set; }
    public string ReceiverEmail { get; set; }
    public List<Domain.User.UserAccessLevel> AccessLevels { get; set; }
    public bool HasAccepted { get; set; }
    public DateTime TokenExpirationTime { get; set; }
    public DateTime RemoveFromDbDate { get; set; }
}