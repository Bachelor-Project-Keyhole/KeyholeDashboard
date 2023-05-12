namespace Repository.Organization;

[BsonCollection("organizations")]
public class OrganizationEntity : Document
{
    public string OrganizationName { get; set; }
}