namespace Application.Organization.Model;

public class OrganizationResponse
{
    public string OrganizationId { get; set; }
    public string OrganizationOwnerId { get; set; }
    public string OrganizationName { get; set; }
    
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
}