namespace Contracts.v1.Dashboard;

public class CreateDashboardRequest
{
    public string OrganizationId { get; set; }
    public string DashboardName { get; set; }
}