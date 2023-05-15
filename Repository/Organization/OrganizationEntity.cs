#pragma warning disable CS0108, CS0114
#pragma warning disable CS8618
namespace Repository.Organization;

[BsonCollection("organizations")]
public class OrganizationEntity : Document
{
    public string OrganizationOwnerId { get; set; }
    public string OrganizationName { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
}