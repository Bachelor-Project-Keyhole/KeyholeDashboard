using Domain.DomainEntities;
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace Application.User.Model;

public class AdminAndOrganizationCreateResponse
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public DateTime UserCreationTime { get; set; }
    public List<UserAccessLevel> AccessLevels { get; set; }

    public string OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public string Country { get; set; }
    public string Address { get; set; }
    public DateTime OrganizationCreationTime { get; set; }
}