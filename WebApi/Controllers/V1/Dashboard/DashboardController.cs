using System.Net;
using Application.JWT.Authorization;
using AutoMapper;
using Contracts.Dashboard;
using Domain.Dashboard;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Dashboard;

[Route("api/v1/[controller]")]
public class DashboardController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly IDashboardDomainService _dashboardDomainService;
    
    
    public DashboardController(
        IMapper mapper,
        IDashboardDomainService dashboardDomainService)
    {
        _mapper = mapper;
        _dashboardDomainService = dashboardDomainService;
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
    [Route("all")]
    public async Task<IActionResult> GetAllDashboardsOfOrganization(string organizationId)
    {
        var dashboards = await _dashboardDomainService.GetAllDashboards(organizationId);
        return Ok(_mapper.Map<List<DashboardResponse>>(dashboards));
    }
    
    /// <summary>
    /// Create a dashboard (Editor, Admin)
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="dashboardName"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Create a dashboard", typeof(DashboardResponse))]
    [Route("")]
    public async Task<IActionResult> CreateDashboard(string organizationId, string dashboardName)
    {
        var dashboard = await _dashboardDomainService.CreateDashboard(organizationId, dashboardName);
        return Ok(_mapper.Map<DashboardResponse>(dashboard));
    }

    /// <summary>
    /// Update a dashboard (Editor, Admin)
    /// </summary>
    /// <param name="dashboardId"> dashboard id that is being updated </param>
    /// <param name="dashboardName"> new dashboard name </param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Editor, UserAccessLevel.Admin)]
    [HttpPut]
    [SwaggerResponse((int) HttpStatusCode.OK, "Update a dashboard", typeof(DashboardResponse))]
    [Route("")]
    public async Task<IActionResult> UpdateDashboard(string dashboardId, string dashboardName)
    {
        var dashboard = await _dashboardDomainService.UpdateDashboard(dashboardId, dashboardName);
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
        await _dashboardDomainService.RemoveDashboard(dashboardId);
        return Ok();
    }
}