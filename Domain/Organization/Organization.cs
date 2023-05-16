

// ReSharper disable UnusedAutoPropertyAccessor.Global

using Domain.User;

#pragma warning disable CS8618
namespace Domain.Organization;

public class Organization
{
    public string Id { get; set; }
    public string OrganizationOwnerId { get; set; }
    public string OrganizationName { get; set; }
    // ReSharper disable once CollectionNeverQueried.Global
    public List<OrganizationMembers>? Members { get; set; }
    public List<OrganizationDashboards>? Dashboards { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }

}


public class OrganizationMembers
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<UserAccessLevel> AccessLevel { get; set; }
}

public class OrganizationDashboards
{
    public string Id { get; set; }
    public string Name { get; set; }
}




