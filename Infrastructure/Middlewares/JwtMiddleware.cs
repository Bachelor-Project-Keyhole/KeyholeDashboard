using Application.JWT.Authorization;
using Application.JWT.Model;
using Application.JWT.Service;
using Application.User.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtSettings _jwtSettings;
    private readonly IUserService _userService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public JwtMiddleware(
        RequestDelegate next,
        IOptions<JwtSettings> jwtSettings,
        IUserService userService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _next = next;
        _jwtSettings = jwtSettings.Value;
        _userService = userService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task Invoke(HttpContext context)
    {
        using (var scope = context.RequestServices.CreateScope())
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var user = _jwtTokenGenerator.ValidateToken(token);
            if (user != null)
            {
                // attach user and access level information to context on successful jwt validation
                context.Items["User"] = await _userService.GetUserById(user.Id);
                // check access level
                var accessLevels = user.AccessLevel;
                var authorizeAttribute = context.GetEndpoint()?.Metadata.GetMetadata<AuthorizationAttribute>()?.GetAccessLevel();
                if (authorizeAttribute != null && !accessLevels.Any(x => authorizeAttribute.Contains(x)))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
            }
        }
        
        await _next(context);
    }

}