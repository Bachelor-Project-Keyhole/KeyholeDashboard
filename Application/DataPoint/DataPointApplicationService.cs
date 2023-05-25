using Contracts.v1.Dashboard;
using Domain.Datapoint;
using Domain.Template;

namespace Application.DataPoint;

public class DataPointApplicationService : IDataPointApplicationService
{
    private readonly IDataPointDomainService _dataPointDomainService;
    private readonly ITemplateDomainService _templateDomainService;

    public DataPointApplicationService(
        IDataPointDomainService dataPointDomainService,
        ITemplateDomainService templateDomainService)
    {
        _dataPointDomainService = dataPointDomainService;
        _templateDomainService = templateDomainService;
    }
    
    public async Task<DashboardAndElementsResponse> TemplateDataPointsAndEntries(Domain.Dashboard.Dashboard dashboard, List<Template> templates)
    {
        var response = new DashboardAndElementsResponse
        {
            DashboardId = dashboard.Id,
            DashboardName = dashboard.Name,
            Placeholders = new List<Placeholders>()
        };

        foreach (var template in templates)
        {
            
            var dataPointLatestValue = await _templateDomainService.GetLatestValueWithChange(template.DatapointId, template.TimePeriod, template.TimeUnit);
            var dataEntries = await _dataPointDomainService.GetDataPointEntries(
                dashboard.OrganizationId,
                dataPointLatestValue.dataPointKey, 
                Domain.TimeSpanConverter.CalculatePeriodBoundary(template.TimePeriod, template.TimeUnit));
            
            var placeholderData = new Placeholders()
            {
                PositionHeight = template.PositionHeight,
                PositionWidth = template.PositionWidth,
                SizeHeight = template.SizeHeight,
                SizeWidth = template.SizeWidth,
                TemplateId = template.Id,
                Change = dataPointLatestValue.Change,
                Comparison = dataPointLatestValue.ComparisonIsAbsolute,
                IsDirectionUp = dataPointLatestValue.DirectionIsUp,
                LatestValue = dataPointLatestValue.LatestValue,
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