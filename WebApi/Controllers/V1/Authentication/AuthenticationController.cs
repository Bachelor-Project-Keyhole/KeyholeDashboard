using System.Net;
using Application.Authentication.AuthenticationService;
using Application.JWT.Model;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Authentication;

[Route("api/v1/[controller]")]
public class AuthenticationController : BaseApiController
{
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IMapper _mapper;

    public AuthenticationController(
        IUserAuthenticationService userAuthenticationService,
        IMapper mapper)
    {
        _userAuthenticationService = userAuthenticationService;
        _mapper = mapper;
    }


    /// <summary>
    /// Login/Authenticate
    /// </summary>
    /// <param name="loginRequest"> Credentials </param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Login/Authenticate")]
    [Route("login")]
    public async Task<IActionResult> Authenticate(AuthenticateRequest loginRequest)
    {
        var response = await _userAuthenticationService.Authenticate(loginRequest);
        return Ok(response);
    }
}