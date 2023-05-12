using Domain.DomainEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8600

namespace Application.JWT.Authorization;

// For different accessibility to APIs based on access level, custom authorization attribute is required.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizationAttribute : Attribute, IAuthorizationFilter
{

    private readonly UserAccessLevel[] _accessLevels;

    public AuthorizationAttribute(params UserAccessLevel[] accessLevels)
    {
        _accessLevels = accessLevels;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // If api header has anonymous custom-made "AllowAnonymous" attribute, authorization is not required.
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        // Authorization
        // TODO: Create different level of access based on access level enum value
        
        var user = (Domain.DomainEntities.User) context.HttpContext.Items["User"];
        if (user == null || !_accessLevels.Any(requiredLevels => user.AccessLevels.Contains(requiredLevels)))
        {
            context.Result = new JsonResult(new {message = "Unauthorized"})
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }

    public UserAccessLevel[] GetAccessLevel()
    {
        return (UserAccessLevel[])_accessLevels.Clone();
    }
}