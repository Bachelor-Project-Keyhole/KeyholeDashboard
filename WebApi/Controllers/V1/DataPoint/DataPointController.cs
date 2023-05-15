using AutoMapper;
using Contracts;
using Domain.Datapoint;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("{organizationId}")]
    public async Task<ActionResult<DataPointWithValueDto[]>> GetAllDataPointsWithLatestValues(string organizationId)
    {
        var allDataPoints = await _dataPointDomainService.GetAllDataPoints(organizationId);
        return _mapper.Map<DataPointWithValueDto[]>(allDataPoints);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateDataPoint([FromBody] DataPointDto dataPointDto)
    {
        var dataPoint = _mapper.Map<Domain.Datapoint.DataPoint>(dataPointDto);
        await _dataPointDomainService.UpdateDataPoint(dataPoint);
        return Ok();
        
    }
    
    [HttpPost("entries")]
    public async Task<IActionResult> PostDataPointEntry([FromBody] DataPointEntryDto dataPointEntryDto)
    {
        var dataPointEntry = _mapper.Map<DataPointEntry>(dataPointEntryDto);
        await _dataPointDomainService.AddDataPointEntry(dataPointEntry);
        return Ok();
        
    }

    [HttpGet("entries/last/{organizationId}/{dataPointKey}")]
    public async Task<ActionResult<DataPointEntryDto>> GetLatestDataPointEntry(string organizationId, string dataPointKey)
    {
        var dataPointEntry = await _dataPointDomainService.GetLatestDataPointEntry(organizationId, dataPointKey);
        return _mapper.Map<DataPointEntryDto>(dataPointEntry);
    }

    [HttpGet("entries/{organizationId}/{key}")]
    public async Task<ActionResult<DataPointEntryDto[]>> GetAllDataPointEntries(string organizationId, string key)
    {
        var allDataPoints = await _dataPointDomainService.GetAllDataPointEntries(organizationId, key);
        return _mapper.Map<DataPointEntryDto[]>(allDataPoints);
    }
}