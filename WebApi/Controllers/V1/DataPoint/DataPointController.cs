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

    /// <summary>
    /// Create Data Point
    /// </summary>
    /// <param name="createDataPointDto">Operation: None, Add, Subtract, Multiply, Divide </param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<DataPointDto>> CreateDataPoint([FromBody] CreateDataPointDto createDataPointDto)
    {
        var dataPoint = _mapper.Map<Domain.Datapoint.DataPoint>(createDataPointDto);
        var result = await _dataPointDomainService.CreateDataPoint(dataPoint);
        return Ok(result);
    }

    /// <summary>
    /// Get all data points belonging to the organization
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    [HttpGet("{organizationId}")]
    public async Task<ActionResult<DataPointDto[]>> GetAllDataPointsWithLatestValues(string organizationId)
    {
        var dataPoints = await _dataPointDomainService.GetAllDataPoints(organizationId);
        return _mapper.Map<DataPointDto[]>(dataPoints);
    }

    /// <summary>
    /// Update Data Point
    /// </summary>
    /// <param name="dataPointDto">Operation: None, Add, Subtract, Multiply, Divide </param>
    /// <returns></returns>
    [HttpPatch]
    public async Task<IActionResult> UpdateDataPoint([FromBody] DataPointDto dataPointDto)
    {
        var dataPoint = _mapper.Map<Domain.Datapoint.DataPoint>(dataPointDto);
        await _dataPointDomainService.UpdateDataPoint(dataPoint);
        return Ok();
        
    }
    
    /// <summary>
    /// Post Data Point entry. If data point key is unique, new data point will be created with this key
    /// </summary>
    /// <param name="dataPointEntryDto"></param>
    /// <returns></returns>
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