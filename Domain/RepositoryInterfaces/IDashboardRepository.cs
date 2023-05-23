namespace Domain.RepositoryInterfaces;

public interface IDashboardRepository
{
    Task<Dashboard.Dashboard?> GetDashboardById(string dashboardId);
    Task<List<Dashboard.Dashboard>?> GetAllDashboards(string organizationId);
    Task Insert(Dashboard.Dashboard dashboard);
    Task Update(Dashboard.Dashboard dashboard);
    Task Delete(string id);
}