namespace Application.Organization.Model;

public class AllUsersOfOrganizationResponse
{
    public string OrganizationId { get; set; }
    public List<OrganizationUsersResponse>? Users { get; set; }
}

public class OrganizationUsersResponse
{
    public string Name { get; set; }
    public string Email { get; set; }
    public List<Domain.User.UserAccessLevel> AccessLevels { get; set; }
    
}