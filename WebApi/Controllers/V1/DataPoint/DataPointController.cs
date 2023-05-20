using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts;
using Domain.Datapoint;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers.V1.DataPoint;

[Route("api/v1/[controller]")]
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
    
    /// <summary>
    /// Post Data Point entry. If data point key is unique, new data point will be created with this key (needed access Editor or Admin)
    /// </summary>
    /// <param name="dataPointEntryDto"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.OK, "Update Data Point")]
    [Route("entries")]
    public async Task<IActionResult> PostDataPointEntry([FromBody] DataPointEntryDto dataPointEntryDto)
    {
        var dataPointEntry = _mapper.Map<DataPointEntry>(dataPointEntryDto);
        await _dataPointDomainService.AddDataPointEntry(dataPointEntry);
        return Ok();
        
    }

    /// <summary>
    /// Get latest data point entry (Any auth level is required)
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="dataPointKey"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "Get latest data point entry")]
    [Route("entries/last/{organizationId}/{dataPointKey}")]
    public async Task<ActionResult<DataPointEntryDto>> GetLatestDataPointEntry(string organizationId, string dataPointKey)
    {
        var dataPointEntry = await _dataPointDomainService.GetLatestDataPointEntry(organizationId, dataPointKey);
        return _mapper.Map<DataPointEntryDto>(dataPointEntry);
    }

    /// <summary>
    /// Get all data point entries (Any auth level is required)
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "Get all data point entries")]
    [Route("entries/{organizationId}/{key}")]
    public async Task<ActionResult<DataPointEntryDto[]>> GetAllDataPointEntries(string organizationId, string key)
    {
        var allDataPoints = await _dataPointDomainService.GetAllDataPointEntries(organizationId, key);
        return _mapper.Map<DataPointEntryDto[]>(allDataPoints);
    }
}