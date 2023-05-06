using AutoMapper;
using Contracts;
using Domain.Datapoint;
using Domain.Exceptions;
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
    public async Task<ActionResult<DataPointDto[]>> GetAllDataPoints(string organizationId)
    {
        try
        {
            var allDataPoints = await _dataPointDomainService.GetAllDataPoints(organizationId);
            return _mapper.Map<DataPointDto[]>(allDataPoints);
        }
        catch (OrganizationNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost("entries")]
    public async Task<IActionResult> PostDataPointEntry([FromBody] DataPointEntryDto dataPointEntryDto)
    {
        try
        {
            var dataPointEntry = _mapper.Map<DataPointEntry>(dataPointEntryDto);
            await _dataPointDomainService.AddDataPointEntry(dataPointEntry);
            return Ok();
        }
        catch (OrganizationNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }
}
