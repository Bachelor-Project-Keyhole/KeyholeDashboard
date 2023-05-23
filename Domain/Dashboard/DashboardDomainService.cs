using Domain.Exceptions;
using Domain.RepositoryInterfaces;

namespace Domain.Dashboard;

public class DashboardDomainService : IDashboardDomainService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardDomainService(
        IOrganizationRepository organizationRepository,
        IDashboardRepository dashboardRepository)
    {
        _organizationRepository = organizationRepository;
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Dashboard> GetDashboardById(string id)
    {
        var dashboard = await _dashboardRepository.GetDashboardById(id);
        if(dashboard == null)
            throw new DashboardNotFoundException($"Dashboard with id: {id} was not found");
        return dashboard;
    }

    public async Task<List<Dashboard>?> GetAllDashboards(string organizationId)
    {
        var dashboards = await _dashboardRepository.GetAllDashboards(organizationId);
        if (dashboards == null)
            throw new DashboardNotFoundException($"No dashboards were found with organization Id: {organizationId}");
        return dashboards;
    }

    public async Task<Dashboard> CreateDashboard(string organizationId, string dashboardName)
    {
        var exist = await _organizationRepository.OrganizationExists(organizationId);
        if (!exist)
            throw new OrganizationNotFoundException($"Organization with {organizationId} was not found");

        var insert = new Dashboard
        {
            Id = IdGenerator.GenerateId(),
            Name = dashboardName,
            OrganizationId = organizationId,
        };

        await _dashboardRepository.Insert(insert);
        return insert;
    }

    public async Task<Dashboard> UpdateDashboard(string dashboardId, string dashboardName)
    {
        var dashboard = await _dashboardRepository.GetDashboardById(dashboardId);
        if (dashboard == null)
            throw new DashboardNotFoundException($"Dashboard with id: {dashboardId} was not found");

        dashboard.Name = dashboardName;
        await _dashboardRepository.Update(dashboard);
        return dashboard;
    }

    public async Task RemoveDashboard(string id)
    {
        var dashboard = await _dashboardRepository.GetDashboardById(id);
        if (dashboard == null)
            throw new DashboardNotFoundException($"Dashboard with id: {id} was not found");
        await _dashboardRepository.Delete(id);
    }
}