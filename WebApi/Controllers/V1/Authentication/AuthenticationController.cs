using System.Net;
using Application.Authentication.AuthenticationService;
using Application.JWT.Authorization;
using Application.User.UserService;
using AutoMapper;
using Contracts.v1.Authentication;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Authentication;
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
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
    /// Login/Authenticate
    /// </summary>
    /// <param name="loginRequest"> Credentials </param>z
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
    /// Rotate Refresh token if it is still active (refresh token has to be in cookies)
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Rotate Refresh token if it is still active", typeof(AuthenticationResponse))]
    [Route("token/refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        var response = await _userAuthenticationService.RefreshToken(refreshToken);
        return Ok(response);
    }
    
    /// <summary>
    /// Refresh token when cookies disabled
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Rotate Refresh token if it is still active", typeof(AuthenticationResponse))]
    [Route("token/refresh/cookie")]
    public async Task<IActionResult> RefreshTokenNonCookie(AddNonRefreshTokenRequest request)
    {
        var response = await _userAuthenticationService.RefreshToken(request.Token);
        return Ok(response);
    }

    /// <summary>
    /// Logout / Revoke token
    /// </summary>
    /// <param name="request"> Refresh token(this is null in case the token can be retrieved from cookies)</param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin, UserAccessLevel.Editor, UserAccessLevel.Viewer)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Logout / Revoke token", typeof(AuthenticationResponse))]
    [Route("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        await _userService.Revoke(request);
        return Ok(new {message = "Token revoked"});
    }
    
}

