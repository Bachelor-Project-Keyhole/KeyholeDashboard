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
    /// <param name="timeSpanInDays">Returns values that occured this amount of days ago until today</param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "Get data point entry values for template")]
    [Route("{organizationId}/{dataPointId}")]
    public async Task<DataPointEntryDto[]> GetDataForTemplate(string organizationId, string dataPointId,
        [FromQuery] string displayType, [FromQuery] int timeSpanInDays)
    {
        var dataPointEntries = await _templateDomainService.GetDataForTemplate(organizationId,
            dataPointId, displayType, timeSpanInDays);
        return _mapper.Map<DataPointEntryDto[]>(dataPointEntries);
    }
}