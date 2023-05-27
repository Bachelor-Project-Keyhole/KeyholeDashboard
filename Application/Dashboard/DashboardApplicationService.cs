using Contracts.v1.Dashboard;
using Domain.Dashboard;
using Domain.Datapoint;
using Domain.Template;

namespace Application.Dashboard;

public class DashboardApplicationService : IDashboardApplicationService
{
    private readonly IDataPointDomainService _dataPointDomainService;
    private readonly IDashboardDomainService _dashboardDomainService;
    private readonly ITemplateDomainService _templateDomainService;

    public DashboardApplicationService(
        IDataPointDomainService dataPointDomainService,
        ITemplateDomainService templateDomainService,
        IDashboardDomainService dashboardDomainService)
    {
        _dataPointDomainService = dataPointDomainService;
        _templateDomainService = templateDomainService;
        _dashboardDomainService = dashboardDomainService;
    }
    
    
    public async Task<DashboardAndElementsResponse> LoadDashboard(string dashboardId)
    {
        var dashboard = await _dashboardDomainService.GetDashboardById(dashboardId);
        var templates = await _templateDomainService.GetAllByDashboardId(dashboardId);
        
        var response = new DashboardAndElementsResponse
        {
            DashboardId = dashboard.Id,
            DashboardName = dashboard.Name,
            Placeholders = new List<Placeholders>()
        };

        foreach (var template in templates)
        {
            
            var latestValueWithChange = await _templateDomainService.GetLatestValueWithChange(template.DatapointId, template.TimePeriod, template.TimeUnit);
            var dataEntries = await _dataPointDomainService.GetDataPointEntries(
                dashboard.OrganizationId,
                latestValueWithChange.DataPointKey, 
                Domain.TimeSpanConverter.CalculatePeriodBoundary(template.TimePeriod, template.TimeUnit));
            
            var placeholderData = new Placeholders
            {
                PositionHeight = template.PositionHeight,
                PositionWidth = template.PositionWidth,
                SizeHeight = template.SizeHeight,
                SizeWidth = template.SizeWidth,
                TemplateId = template.Id,
                Change = latestValueWithChange.Change,
                Comparison = latestValueWithChange.ComparisonIsAbsolute,
                IsDirectionUp = latestValueWithChange.DirectionIsUp,
                DisplayName = latestValueWithChange.DisplayName,
                LatestValue = latestValueWithChange.LatestValue,
                Values = dataEntries.Select(x => new ValueResponse
                {
                    Value = x.Value,
                    Time = x.Time
                }).ToList(),
            };
            
            response.Placeholders.Add(placeholderData);
        }
        return response;
    }
}
