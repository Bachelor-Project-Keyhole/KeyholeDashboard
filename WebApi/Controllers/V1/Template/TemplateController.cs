using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts.v1.DataPoint;
using Contracts.v1.Template;
using Domain.Template;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Template;

[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
public class TemplateController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly ITemplateDomainService _templateDomainService;

    public TemplateController(
        IMapper mapper,
        ITemplateDomainService templateDomainService)
    {
        _mapper = mapper;
        _templateDomainService = templateDomainService;
    }
    
    /// <summary>
    /// Get data point entry values for template (Any access level will work)
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="dataPointId"></param>
    /// <param name="displayType"></param>
    /// <param name="timePeriod">Returns values that occured this amount of days ago until today</param>
    /// <param name="timeUnit"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "Get data point entry values for template")]
    [Route("{organizationId}/{dataPointId}")]
    public async Task<DataPointEntryResponse[]> GetDataForTemplate(string organizationId, string dataPointId,
        [FromQuery] string displayType, [FromQuery] int timePeriod, [FromQuery] TimeUnit timeUnit)
    {
        var dataPointEntries = await _templateDomainService.GetDataForTemplate(organizationId,
            dataPointId, displayType, timePeriod, timeUnit);
        return _mapper.Map<DataPointEntryResponse[]>(dataPointEntries);
    }

    /// <summary>
    /// Get latest with change from previous period  
    /// </summary>
    /// <param name="dataPointId"></param>
    /// <param name="timePeriod">The amount of days back in time to compare current value with</param>
    /// <param name="timeUnit"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet("latest-value-with-change/{dataPointId}")]
    public async Task<LatestValueWithChangeResponse> GetLatestValueWithChange(string dataPointId,
        [FromQuery] int timePeriod, [FromQuery] TimeUnit timeUnit)
    {
        var latestValueWithChange =
            await _templateDomainService.GetLatestValueWithChange(dataPointId, timePeriod, timeUnit);
        return new LatestValueWithChangeResponse
        {
            LatestValue = latestValueWithChange.LatestValue,
            Change = latestValueWithChange.Change,
            DirectionIsUp = latestValueWithChange.DirectionIsUp,
            ComparisonIsAbsolute = latestValueWithChange.ComparisonIsAbsolute
        };
    }

    /// <summary>
    /// Get template by Id (Any access level)
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get template by id", typeof(TemplateResponse))]
    [Route("{templateId}")]
    public async Task<IActionResult> GetTemplateById(string templateId)
    {
        var template = await _templateDomainService.GetTemplateById(templateId);
        return Ok(_mapper.Map<TemplateResponse>(template));
    }

    /// <summary>
    /// Get all templates by dashboard id
    /// </summary>
    /// <param name="dashboardId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get all templates by dashboard id", typeof(List<TemplateResponse>))]
    [Route("all/{dashboardId}")]
    public async Task<IActionResult> GetAllTemplatesByDashboardId(string dashboardId)
    {
        var templates = await _templateDomainService.GetAllByDashboardId(dashboardId);
        return Ok(_mapper.Map<List<TemplateResponse>>(templates));
    }
    
    
    /// <summary>
    /// Create a template (Editor, Admin)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Create a template", typeof(TemplateResponse))]
    [Route("")]
    public async Task<IActionResult> CreateTemplate(CreateTemplateRequest request)
    {
        var domain = _mapper.Map<Domain.Template.Template>(request);
        var template = await _templateDomainService.CreateTemplate(domain);
        return Ok(_mapper.Map<TemplateResponse>(template));
    }


    /// <summary>
    /// Update a template (Editor, Admin)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPut]
    [SwaggerResponse((int) HttpStatusCode.OK, "Update template", typeof(TemplateResponse))]
    [Route("")]
    public async Task<IActionResult> UpdateTemplate(UpdateTemplateRequest request)
    {
        var domain = _mapper.Map<Domain.Template.Template>(request);
        var template = await _templateDomainService.Update(domain);
        return Ok(_mapper.Map<TemplateResponse>(template));
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpDelete]
    [SwaggerResponse((int) HttpStatusCode.OK, "Delete template")]
    [Route("{id}")]
    public async Task<IActionResult> DeleteTemplate(string id)
    {
        await _templateDomainService.RemoveTemplate(id);
        return Ok();
    }
}
