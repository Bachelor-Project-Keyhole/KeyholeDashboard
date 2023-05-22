using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts;
using Domain.Template;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers.V1;

[Route("api/v1/[controller]")]
public class TemplateController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ITemplateDomainService _templateDomainService;

    public TemplateController(IMapper mapper, ITemplateDomainService templateDomainService)
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
    public async Task<DataPointEntryDto[]> GetDataForTemplate(string organizationId, string dataPointId,
        [FromQuery] string displayType, [FromQuery] int timePeriod, [FromQuery] TimeUnit timeUnit)
    {
        var dataPointEntries = await _templateDomainService.GetDataForTemplate(organizationId,
            dataPointId, displayType, timePeriod, timeUnit);
        return _mapper.Map<DataPointEntryDto[]>(dataPointEntries);
    }

    /// <summary>
    /// Get latest with change from previous period  
    /// </summary>
    /// <param name="dataPointId"></param>
    /// <param name="timePeriod">The amount of days back in time to compare current value with</param>
    /// <param name="timeUnit"></param>
    /// <returns></returns>
    [HttpGet("latest-value-with-change/{dataPointId}")]
    public async Task<LatestValueWithChangeDto> GetLatestValueWithChange(string dataPointId,
        [FromQuery] int timePeriod, [FromQuery] TimeUnit timeUnit)
    {
        var latestValueWithChange =
            await _templateDomainService.GetLatestValueWithChange(dataPointId, timePeriod, timeUnit);
        return new LatestValueWithChangeDto
        {
            LatestValue = latestValueWithChange.LatestValue,
            Change = latestValueWithChange.Change
        };
    }
}