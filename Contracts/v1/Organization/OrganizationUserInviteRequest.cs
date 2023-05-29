namespace Contracts.v1.Organization;

public class OrganizationUserInviteRequest
{
    public string OrganizationId { get; set; }
    public string ReceiverEmailAddress { get; set; }
    public string AccessLevel { get; set; }
    public string? Message { get; set; } // if this is not null, it will overhaul default message
}