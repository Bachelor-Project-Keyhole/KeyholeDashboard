using System.Net;
using Application.Authentication.AuthenticationService;
using Application.JWT.Authorization;
using Application.JWT.Model;
using Application.User.Model;
using Application.User.UserService;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Authentication;

[Route("api/v1/[controller]")]
public class AuthenticationController : BaseApiController
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public AuthenticationController(
        IUserAuthenticationService userAuthenticationService,
        IMapper mapper,
        IUserService userService)
    {
        _userAuthenticationService = userAuthenticationService;
        _mapper = mapper;
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
        var response = await _userService.CreateAdminUserAndOrganization(request);
        return Ok(response);
    }

    /// <summary>
    /// Login/Authenticate
    /// </summary>
    /// <param name="loginRequest"> Credentials </param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Login/Authenticate", typeof(AuthenticationResponse))]
    [Route("login")]
    public async Task<IActionResult> Authenticate(AuthenticateRequest loginRequest)
    {
        var response = await _userAuthenticationService.Authenticate(loginRequest);
        return Ok(response);
    }

    /// <summary>
    /// Logout / Revoke token
    /// </summary>
    /// <param name="request"> If cookies qre disabled, token can be given manually </param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Logout / Revoke token", typeof(AuthenticationResponse))]
    [Route("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        await _userService.Revoke(request);
        return Ok(new {message = "Token revoked"});
    }

    /// <summary>
    /// Change access level to user
    /// </summary>
    /// <param name="request">
    /// AdminId -> admin that wants to change access to user
    /// UserId -> id of user that accesses will be changed
    /// SetAccessLevel -> to level the user access should be set
    /// </param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Change access level to user",typeof(UserChangeAccessResponse))]
    [Route("access/{id}")]
    public async Task<IActionResult> ChangeAccessLevelOfUser([FromBody] ChangeUserAccessRequest request)
    {
        var response = await _userService.SetAccessLevel(request);
        return Ok(response);
    }
}