using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts;
using Domain.Datapoint;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers.Public;

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
    /// <param name="pushDataPointEntryDto"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.OK, "Post Data Point Entry")]
    [Route("entries")]
    public async Task<IActionResult> PostDataPointEntry([FromBody] PushDataPointEntryDto pushDataPointEntryDto)
    {
        var dataPointEntry = _mapper.Map<DataPointEntry>(pushDataPointEntryDto);
        await _dataPointDomainService.AddDataPointEntry(dataPointEntry);
        return Ok();
    }
}