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
        await _dataPointDomainService.AddDataPointEntry(pushDataPointEntryDto.DataPointKey, pushDataPointEntryDto.Value,
            apiKey);
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
}