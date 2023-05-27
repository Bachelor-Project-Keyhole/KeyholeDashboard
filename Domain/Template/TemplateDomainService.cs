using Domain.Datapoint;
using Domain.Exceptions;
using Domain.RepositoryInterfaces;

namespace Domain.Template;

public class TemplateDomainService : ITemplateDomainService
{
    private readonly IDataPointDomainService _dataPointDomainService;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IDataPointRepository _dataPointRepository;
    private readonly ITemplateRepository _templateRepository;

    public TemplateDomainService(
        IDataPointDomainService dataPointDomainService,
        IDashboardRepository dashboardRepository,
        IDataPointRepository dataPointRepository,
        ITemplateRepository templateRepository)
    {
        _dataPointDomainService = dataPointDomainService;
        _dashboardRepository = dashboardRepository;
        _dataPointRepository = dataPointRepository;
        _templateRepository = templateRepository;
    }

    public async Task<DataPointEntry[]> GetDataForTemplate(string organizationId, string dataPointId,
        string displayType, int timeSpan, TimeUnit timeUnit)
    {
        var dataPoint = await _dataPointDomainService.GetDataPointById(dataPointId);
        var periodStartDate = TimeSpanConverter.CalculatePeriodBoundary(timeSpan, timeUnit);
        var dataPointEntries =
            await _dataPointDomainService.GetDataPointEntries(organizationId, dataPoint.DataPointKey, periodStartDate);
        CalculateEntryValuesBasedOnFormula(dataPoint, dataPointEntries);
        return dataPointEntries;
    }

    public async Task<LatestValuewithChange> GetLatestValueWithChange(string dataPointId, int timeSpan,
            TimeUnit timeUnit)
    {
        var dataPoint = await _dataPointDomainService.GetDataPointById(dataPointId);
        var endOfPeriod = TimeSpanConverter.CalculatePeriodBoundary(timeSpan, timeUnit);
        var change = await _dataPointDomainService.CalculateChangeOverTime(dataPoint, endOfPeriod);
        return new LatestValuewithChange
        {
            DataPointKey = dataPoint.DataPointKey,
            DisplayName = dataPoint.DisplayName,
            LatestValue = dataPoint.LatestValue,
            Change = change,
            DirectionIsUp = dataPoint.DirectionIsUp,
            ComparisonIsAbsolute = dataPoint.ComparisonIsAbsolute
        };
    }

    public async Task<Template> GetTemplateById(string id)
    {
        var template = await _templateRepository.GetById(id);
        if (template == null)
            throw new TemplateNotFoundException($"Template with Id: {id} was not found");

        return template;
    }

    public async Task<List<Template>> GetAllByDashboardId(string dashboardId)
    {
        var dashboard = await _dashboardRepository.GetDashboardById(dashboardId);
        if(dashboard == null)
            throw new DashboardNotFoundException($"Dashboard with Id: {dashboardId} does not exist");

        var templates = await _templateRepository.GetAllByDashboardId(dashboardId);
        if(templates == null || templates.Count < 1)
            throw new TemplateNotFoundException($"Templates with dashboardId: {dashboardId} was not found");

        return templates;
    }

    public async Task<Template> CreateTemplate(Template template)
    {
        ValidateTemplateMetrics(template);
        
        var dashboard = await _dashboardRepository.GetDashboardById(template.DashboardId);
        if (dashboard == null)
            throw new DashboardNotFoundException($"Dashboard with Id: {template.DashboardId} does not exist");
        var datapoint = await _dataPointRepository.GetDataPointById(template.DatapointId);
        if (datapoint == null)
            throw new DataPointNotFoundException(template.DatapointId);

        template.Id = IdGenerator.GenerateId();
        await _templateRepository.Insert(template);
        return template;
    }

    public async Task<Template> Update(Template template)
    {
        ValidateTemplateMetrics(template);
        
        var templateFromDb = await _templateRepository.GetById(template.Id);
        if (templateFromDb == null)
            throw new TemplateNotFoundException($"Template with Id: {template.Id} was not found");

        // Should we allow to change datapoint Id? 
        if (templateFromDb.DatapointId != template.DatapointId)
        {
            var dataPoint = await _dataPointRepository.GetDataPointById(template.DatapointId);
            if (dataPoint == null)
                throw new DataPointNotFoundException(template.DatapointId);
        }

        template.DashboardId = templateFromDb.DashboardId;
        await _templateRepository.Update(template);
        return template;

    }

    public async Task RemoveTemplate(string id)
    {
        var template = await _templateRepository.GetById(id);
        if (template == null)
            throw new TemplateNotFoundException($"Template with Id: {id} was not found");

        await _templateRepository.DeleteTemplate(id);
    }

    public async Task RemoveAllTemplatesWithDashboardId(string dashboardId)
    {
        var dashboard = await _dashboardRepository.GetDashboardById(dashboardId);
        if (dashboard == null)
            throw new DashboardNotFoundException($"Dashboard with Id: {dashboardId} was not found");
        await _templateRepository.RemoveAllTemplatesWithDashboardId(dashboardId);
    }

    private void CalculateEntryValuesBasedOnFormula(DataPoint dataPoint, DataPointEntry[] dataPointEntries)
    {
        foreach (var dataPointEntry in dataPointEntries)
        {
            dataPointEntry.Value = dataPoint.CalculateEntryValueWithFormula(dataPointEntry.Value);
        }
    }

    private void ValidateTemplateMetrics(Template template)
    {
        if (template.PositionHeight < 0)
            throw new InvalidTemplateMetricException($"Position height: {template.PositionHeight} invalid");
        if (template.PositionWidth < 0)
            throw new InvalidTemplateMetricException($"Position width: {template.PositionWidth} invalid");
        if (template.SizeHeight < 1)
            throw new InvalidTemplateMetricException($"Size height: {template.SizeHeight} invalid");
        if (template.SizeWidth < 1)
            throw new InvalidTemplateMetricException($"Size Width: {template.SizeWidth} invalid");
    }
}