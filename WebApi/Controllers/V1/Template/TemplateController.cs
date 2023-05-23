using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts;
using Contracts.Template;
using Domain.Template;
using Domain.User;
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

    /// <summary>
    /// Get template by Id (Any access level)
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get template by id", typeof(TemplateResponse))]
    [Route("{templateId}")]
    public async Task<IActionResult> GetTemplateById(string templateId)
    {
        var template = await _templateDomainService.GetTemplateById(templateId);
        return Ok(_mapper.Map<TemplateResponse>(template));
    }

    /// <summary>
    /// Get all templates by dashboard id
    /// </summary>
    /// <param name="dashboardId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get all templates by dashboard id", typeof(List<TemplateResponse>))]
    [Route("all/{dashboardId}")]
    public async Task<IActionResult> GetAllTemplatesByDashboardId(string dashboardId)
    {
        var templates = await _templateDomainService.GetAllByDashboardId(dashboardId);
        return Ok(_mapper.Map<List<TemplateResponse>>(templates));
    }
    
    
    /// <summary>
    /// Create a template (Editor, Admin)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Create a template", typeof(TemplateResponse))]
    [Route("")]
    public async Task<IActionResult> CreateTemplate(CreateTemplateRequest request)
    {
        var domain = _mapper.Map<Domain.Template.Template>(request);
        var template = await _templateDomainService.CreateTemplate(domain);
        return Ok(_mapper.Map<TemplateResponse>(template));
    }


    /// <summary>
    /// Update a template (Editor, Admin)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPut]
    [SwaggerResponse((int) HttpStatusCode.OK, "Create a template", typeof(TemplateResponse))]
    [Route("")]
    public async Task<IActionResult> UpdateTemplate(UpdateTemplateRequest request)
    {
        var domain = _mapper.Map<Domain.Template.Template>(request);
        var template = await _templateDomainService.Update(domain);
        return Ok(_mapper.Map<TemplateResponse>(template));
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpDelete]
    [SwaggerResponse((int) HttpStatusCode.OK, "Create a template")]
    [Route("{id}")]
    public async Task<IActionResult> DeleteTemplate(string id)
    {
        await _templateDomainService.RemoveTemplate(id);
        return Ok();
    }
}
