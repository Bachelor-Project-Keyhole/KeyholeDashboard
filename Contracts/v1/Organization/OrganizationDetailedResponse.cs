namespace Contracts.v1.Organization;

public class OrganizationDetailedResponse
{
    public string OrganizationId { get; set; }
    public string OrganizationOwnerId { get; set; }
    public string OrganizationName { get; set; }
    public string ApiKey { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
}