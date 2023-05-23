using System.Net;
using AutoMapper;
using Contracts;
using Contracts.Template;
using Domain.Template;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Template;

[Route("api/v1/[controller]")]
public class TemplateController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly ITemplateDomainService _templateDomainService;

    public TemplateController(
        IMapper mapper,
        ITemplateDomainService templateDomainService)
    {
        _mapper = mapper;
        _templateDomainService = templateDomainService;
    }

    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Create a template")]
    [Route("")]
    public async Task<IActionResult> CreateTemplate(CreateTemplateRequest request)
    {
        var domain = _mapper.Map<Domain.Template.Template>(request);
        var template = await _templateDomainService.CreateTemplate(domain);
        return Ok(domain);
    }
}