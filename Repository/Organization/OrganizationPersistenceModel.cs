using Domain.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

#pragma warning disable CS8618

namespace Repository.Organization;
[BsonCollection("organization")]
public class OrganizationPersistenceModel : Document
{
    public string OrganizationOwnerId { get; set; }
    public string OrganizationName { get; set; }
    public List<PersistenceOrganizationMembers>? Members { get; set; }
    public List<PersistenceOrganizationDashboards>? Dashboards { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }
}

public class PersistenceOrganizationMembers
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    [BsonRepresentation(BsonType.String)]
    public List<UserAccessLevel> AccessLevel { get; set; }
}

public class PersistenceOrganizationDashboards
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
}