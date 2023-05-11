using AutoMapper;
using Contracts;
using Domain.Datapoint;
using Domain.Exceptions;
using Domain.Organization;
using Domain.RepositoryInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.V1.DataPoint;

[Route("api/v1/[controller]")]
public class DataPointController : ControllerBase
{
    private readonly IDataPointDomainService _dataPointDomainService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMapper _mapper;

    public DataPointController(IDataPointDomainService dataPointDomainService, IMapper mapper, IOrganizationRepository organizationRepository)
    {
        _dataPointDomainService = dataPointDomainService;
        _mapper = mapper;
        _organizationRepository = organizationRepository;
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
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return StatusCode(500, $"An unexpected error occurred.\n {exception.Message}");
        }
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateDataPoint([FromBody] DataPointDto dataPointDto)
    {
        try
        {
            var dataPoint = _mapper.Map<Domain.Datapoint.DataPoint>(dataPointDto);
            await _dataPointDomainService.UpdateDataPoint(dataPoint);
            return Ok();
        }
        catch (OrganizationNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (DataPointKeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return StatusCode(500, $"An unexpected error occurred.\n {exception.Message}");
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
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return StatusCode(500, $"An unexpected error occurred.\n {exception.Message}");
        }
    }
    
    [HttpGet("{organizationId}/{key}")]
    public async Task<ActionResult<DataPointEntryDto[]>> GetAllDataPointEntries(string organizationId, string key)
    {
        try
        {
            var allDataPoints = await _dataPointDomainService.GetAllDataPointEntries(organizationId, key);
            return _mapper.Map<DataPointEntryDto[]>(allDataPoints);
        }
        catch (OrganizationNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (DataPointKeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return StatusCode(500, $"An unexpected error occurred.\n {exception.Message}");
        }
    }

    [HttpPost("organization/")]
    public async Task<IActionResult> AddOrganization(OrganizationDto organizationDto)
    {
        var organization = _mapper.Map<TempOrganization>(organizationDto);
        await _organizationRepository.Insert(organization);
        return Ok();
    }
}