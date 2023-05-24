using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts.v1.DataPoint;
using Domain.Datapoint;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers.V1.DataPoint;

[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
public class DataPointController : ControllerBase
{
    private readonly IDataPointDomainService _dataPointDomainService;
    private readonly IMapper _mapper;

    public DataPointController(IDataPointDomainService dataPointDomainService, IMapper mapper)
    {
        _dataPointDomainService = dataPointDomainService;
        _mapper = mapper;
    }

    /// <summary>
    /// Create Data Point (Editor or admin access level needed)
    /// </summary>
    /// <param name="createDataPointDto">Operation: None, Add, Subtract, Multiply, Divide </param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.OK, "Create Data Point")]
    [Route("")]
    public async Task<ActionResult<DataPointDto>> CreateDataPoint([FromBody] CreateDataPointDto createDataPointDto)
    {
        var dataPoint = _mapper.Map<Domain.Datapoint.DataPoint>(createDataPointDto);
        var result = await _dataPointDomainService.CreateDataPoint(dataPoint);
        return Ok(result);
    }

    /// <summary>
    /// Get all data points belonging to the organization (Any auth level is required)
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer ,UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "Get all data points belonging to the organization")]
    [Route("{organizationId}")]
    public async Task<ActionResult<DataPointDto[]>> GetAllDataPointsWithLatestValues(string organizationId)
    {
        var dataPoints = await _dataPointDomainService.GetAllDataPoints(organizationId);
        return _mapper.Map<DataPointDto[]>(dataPoints);
    }
    
    /// <summary>
    /// Get Data Point display names and Ids (Any auth level is required)
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK,"")]
    [Route("{organizationId}/displayNames")]
    public async Task<ActionResult<DataPointDisplayNameDto[]>> GetDataPointDisplayNames(string organizationId)
    {
        var dataPoints = await _dataPointDomainService.GetAllDataPoints(organizationId);
        return _mapper.Map<DataPointDisplayNameDto[]>(dataPoints);
    }

    /// <summary>
    /// Update Data Point (needed access Editor or Admin)
    /// </summary>
    /// <param name="dataPointDto">Operation: None, Add, Subtract, Multiply, Divide </param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPatch]
    [SwaggerResponse((int)HttpStatusCode.OK, "Update Data Point")]
    [Route("")]
    public async Task<IActionResult> UpdateDataPoint([FromBody] DataPointDto dataPointDto)
    {
        var dataPoint = _mapper.Map<Domain.Datapoint.DataPoint>(dataPointDto);
        await _dataPointDomainService.UpdateDataPoint(dataPoint);
        return Ok();
        
    }
}