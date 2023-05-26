namespace Application.Dashboard;

public interface IDashboardApplicationService
{
    Task<Contracts.v1.Dashboard.DashboardAndElementsResponse> LoadDashboard(string dashboardId);
}