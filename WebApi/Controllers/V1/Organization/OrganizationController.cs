using System.Net;
using Application.JWT.Authorization;
using Application.Organization;
using Application.Organization.Model;
using Application.User.Model;
using Application.User.UserService;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Organization;


[Route("api/v1/[controller]")]
public class OrganizationController : BaseApiController
{
    private readonly IOrganizationService _organizationService;
    private readonly IUserService _userService;

    public OrganizationController(
        IOrganizationService organizationService,
        IUserService userService)
    {
        _organizationService = organizationService;
        _userService = userService;
    }
    
    /// <summary>
    /// Register an user alongside the organization
    /// </summary>
    /// <param name="request"> User and company registration parameters </param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Register an user alongside the organization", typeof(AdminAndOrganizationCreateResponse))]
    [Route("register")]
    public async Task<IActionResult> CreateAdminUser([FromBody] CreateAdminAndOrganizationRequest request)
    {
        var response = await _userService.CreateAdminUserAndOrganization(request); // Maybe it would be a good idea to separate into 2 endpoints.
        return Ok(response);
    }

    [Authorization(UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Invite user into the organization", typeof(OrganizationUserInviteResponse))]
    [Route("invite")]
    public async Task<IActionResult> InviteUserToOrganization(string userId, OrganizationUserInviteRequest request)
    {
        await _organizationService.InviteUser(request);
        return Ok();
    }
    
}