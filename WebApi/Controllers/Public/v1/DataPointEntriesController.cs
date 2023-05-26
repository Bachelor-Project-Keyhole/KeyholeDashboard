using System.Net;
using AutoMapper;
using Domain.Datapoint;
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
    /// Post Data Point entry. If data point key is unique, new data point will be created with this key
    /// </summary>
    /// <param name="apiKey">Api key assigned to the organization</param>
    /// <param name="pushDataPointEntryRequest">Key-value pair representing a single data point entry</param>
    /// <returns></returns>
    [HttpPost]
    [Route("{apiKey}/single")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Post Data Point Entry")]
    public async Task<IActionResult> PostDataPointEntry(string apiKey,
        [FromBody] PushDataPointEntryRequest pushDataPointEntryRequest)
    {
        await _dataPointDomainService.AddDataPointEntry(
            pushDataPointEntryRequest.DataPointKey,
            pushDataPointEntryRequest.Value,
            apiKey);
        return Ok();
    }

    /// <summary>
    /// Push latest data point entries
    /// </summary>
    /// <remarks>Push data point entries into the system. Each entry is a key value pair.
    /// If any of the provided entries contains a new, unique key, a new data point will be created inside of the system and will be accessible from the Manage Data Points page.
    /// Each entry is assigned a DateTime timestamp that represents the time of creation and the value is going to be displayed as latest value for the data points.
    /// Should be used to push latest data value that are true at the time of making the request.</remarks>
    /// <param name="apiKey">Api key assigned to the organization</param>
    /// <param name="pushDataPointEntryDtos">Array of key value pairs where each item represents a data point entry</param>
    /// <returns></returns>
    [HttpPost]
    [Route("{apiKey}")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Post data point entries")]
    public async Task<IActionResult> PostDataPointEntries(string apiKey,
        [FromBody] PushDataPointEntryRequest[] pushDataPointEntryDtos)
    {
        var dataPointEntries = _mapper.Map<DataPointEntry[]>(pushDataPointEntryDtos);
        await _dataPointDomainService.AddDataPointEntries(dataPointEntries, apiKey);
        return Ok();
    }

    /// <summary>
    /// Post data point entries from the past
    /// </summary>
    /// <remarks> Designed to be used when you need to push entries that do not represent current values but where true sometime in the past.
    /// Meant to be used to send data that was true before starting to use our system. </remarks>
    /// <param name="apiKey">Api key assigned to the organization</param>
    /// <param name="historicDataPointEntryDtos">Array of objects composed of data point key, value, and time when the entry was true.</param>
    /// <returns></returns>
    [HttpPost]
    [Route("{apiKey}/historic")]
    [SwaggerResponse((int)HttpStatusCode.OK, "Post Historic Data Point Entries")]
    public async Task<IActionResult> PostHistoricDataPointEntries(string apiKey,
        [FromBody] HistoricDataPointEntryRequest[] historicDataPointEntryDtos)
    {
        var dataPointEntry = _mapper.Map<DataPointEntry[]>(historicDataPointEntryDtos);
        await _dataPointDomainService.AddHistoricDataPointEntries(dataPointEntry, apiKey);
        return Ok();
    }
}