using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts.v1.Dashboard;
using Domain.Dashboard;
using Domain.Template;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Dashboard;

[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
public class DashboardController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly IDashboardDomainService _dashboardDomainService;
    private readonly ITemplateDomainService _templateDomainService;
    
    
    public DashboardController(
        IMapper mapper,
        IDashboardDomainService dashboardDomainService,
        ITemplateDomainService templateDomainService)
    {
        _mapper = mapper;
        _dashboardDomainService = dashboardDomainService;
        _templateDomainService = templateDomainService;
    }

    /// <summary>
    /// Get dashboard by id (Any access level)
    /// </summary>
    /// <param name="dashboardId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get dashboard by Id", typeof(DashboardResponse))]
    [Route("{dashboardId}")]
    public async Task<IActionResult> GetById(string dashboardId)
    {
        var dashboard = await _dashboardDomainService.GetDashboardById(dashboardId);
        return Ok(_mapper.Map<DashboardResponse>(dashboard));
    }
    
    /// <summary>
    /// Get all organizations (Any access level)
    /// </summary>
    /// <param name="organizationId"> dashboards that belong to this organization id </param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get all dashboards", typeof(List<DashboardResponse>))]
    [Route("all/{organizationId}")]
    public async Task<IActionResult> GetAllDashboardsOfOrganization(string organizationId)
    {
        var dashboards = await _dashboardDomainService.GetAllDashboards(organizationId);
        return Ok(_mapper.Map<List<DashboardResponse>>(dashboards));
    }


    /// <summary>
    /// Create a dashboard (Editor, Admin)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Create a dashboard", typeof(DashboardResponse))]
    [Route("")]
    public async Task<IActionResult> CreateDashboard(CreateDashboardRequest request)
    {
        var dashboard = await _dashboardDomainService.CreateDashboard(request.OrganizationId, request.DashboardName);
        return Ok(_mapper.Map<DashboardResponse>(dashboard));
    }

    /// <summary>
    /// Update a dashboard (Editor, Admin)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPut]
    [SwaggerResponse((int) HttpStatusCode.OK, "Update a dashboard", typeof(DashboardResponse))]
    [Route("")]
    public async Task<IActionResult> UpdateDashboard(UpdateDashboardRequest request)
    {
        var dashboard = await _dashboardDomainService.UpdateDashboard(request.DashboardId, request.DashboardName);
        return Ok(_mapper.Map<DashboardResponse>(dashboard));
    }

    /// <summary>
    /// Delete a dashboard by id (Editor, Admin)
    /// </summary>
    /// <param name="dashboardId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpDelete]
    [SwaggerResponse((int) HttpStatusCode.OK, "Delete dashboard by id")]
    [Route("{dashboardId}")]
    public async Task<IActionResult> DeleteDashboard(string dashboardId)
    {
        await _templateDomainService.RemoveAllTemplatesWithDashboardId(dashboardId);
        await _dashboardDomainService.RemoveDashboard(dashboardId);
        return Ok();
    }
}