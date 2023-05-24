#pragma warning disable CS8618

namespace Repository.Organization;
[BsonCollection("organization")]
public class OrganizationPersistenceModel : Document
{
    public string OrganizationOwnerId { get; set; }
    public string OrganizationName { get; set; }
    public string ApiKey { get; set; }
    
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
}

