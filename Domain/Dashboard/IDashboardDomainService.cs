namespace Domain.Dashboard;

public interface IDashboardDomainService
{
    Task<Dashboard> GetDashboardById(string id); 
    Task<List<Dashboard>?> GetAllDashboards(string organizationId);
    Task<Dashboard> CreateDashboard(string organizationId, string dashboardName);
    Task<Dashboard> UpdateDashboard(string dashboardId, string dashboardName);
    Task RemoveDashboard(string id);
}