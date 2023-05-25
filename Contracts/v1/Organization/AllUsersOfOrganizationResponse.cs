namespace Contracts.v1.Organization;

public class AllUsersOfOrganizationResponse
{
    public string OrganizationId { get; set; }
    public List<OrganizationUsersResponse>? Users { get; set; }
}

public class OrganizationUsersResponse
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public List<Domain.User.UserAccessLevel> AccessLevels { get; set; }
    
}