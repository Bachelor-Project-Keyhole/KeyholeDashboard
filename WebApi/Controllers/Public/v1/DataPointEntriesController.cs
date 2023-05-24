using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts;
using Domain.Datapoint;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers.Public.v1;

[Route("api/public/v1/[controller]")]
[ApiExplorerSettings(GroupName = "public")]
public class DataPointEntriesController : ControllerBase
{
    private readonly IDataPointDomainService _dataPointDomainService;
    private readonly IMapper _mapper;

    public DataPointEntriesController(IDataPointDomainService dataPointDomainService, IMapper mapper)
    {
        _dataPointDomainService = dataPointDomainService;
        _mapper = mapper;
    }

    /// <summary>
    /// Post Data Point entry. If data point key is unique, new data point will be created with this key (needed access Editor or Admin)
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="pushDataPointEntryDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{apiKey}/single")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Post Data Point Entry")]
    public async Task<IActionResult> PostDataPointEntry(string apiKey,
        [FromBody] PushDataPointEntryDto pushDataPointEntryDto)
    {
        var dataPointEntry = _mapper.Map<DataPointEntry>(pushDataPointEntryDto);
        await _dataPointDomainService.AddDataPointEntry(dataPointEntry, apiKey);
        return Ok();
    }
    
    [HttpPost]
    [Route("{apiKey}")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Post Data Point Entries")]
    public async Task<IActionResult> PostDataPointEntries(string apiKey,
        [FromBody] PushDataPointEntryDto[] pushDataPointEntryDtos)
    {
        var dataPointEntries = _mapper.Map<DataPointEntry[]>(pushDataPointEntryDtos);
        await _dataPointDomainService.AddDataPointEntries(dataPointEntries, apiKey);
        return Ok();
    }

    [HttpPost]
    [Route("{apiKey}/historic")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Post Historic Data Point Entries")]
    public async Task<IActionResult> PostHistoricDataPointEntries(string apiKey,
        [FromBody] HistoricDataPointEntryDto[] historicDataPointEntryDtos)
    {
        var dataPointEntry = _mapper.Map<DataPointEntry[]>(historicDataPointEntryDtos);
        await _dataPointDomainService.AddHistoricDataPointEntries(dataPointEntry, apiKey);
        return Ok();
    }

    #region Debug endpoints

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
    public async Task<ActionResult<DataPointEntryDto>> GetLatestDataPointEntry(string organizationId,
        string dataPointKey)
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
    public async Task<ActionResult<PushDataPointEntryDto[]>> GetAllDataPointEntries(string organizationId, string key)
    {
        var allDataPoints = await _dataPointDomainService.GetAllDataPointEntries(organizationId, key);
        return _mapper.Map<PushDataPointEntryDto[]>(allDataPoints);
    }

    #endregion
}