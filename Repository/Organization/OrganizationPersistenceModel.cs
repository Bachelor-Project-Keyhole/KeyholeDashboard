using Domain.User;
using MongoDB.Bson;

#pragma warning disable CS8618

namespace Repository.Organization;
[BsonCollection("organization")]
public class OrganizationPersistenceModel : Document
{
    public string OrganizationOwnerId { get; set; }
    public string OrganizationName { get; set; }
    public string Country { get; set; }
    public string Address { get; set; }
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
    public List<UserAccessLevel> AccessLevel { get; set; }
}

public class PersistenceOrganizationDashboards
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
}