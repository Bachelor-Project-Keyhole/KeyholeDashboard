using System.Net;
using System.Text.Json;
using Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            switch (exception)
            {
                #region User Exceptions 
                
                case UserEmailTakenException:
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    break;
                case UserNotFoundException:
                    response.StatusCode = (int) HttpStatusCode.NotFound;
                    break;
                case UserInvalidActionException:
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    break;
                case UserForbiddenAction:
                    response.StatusCode = (int) HttpStatusCode.Forbidden;
                    break;
                case PasswordTooShortException:
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    break;
                case InvitationTokenException:
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    break;
                case InvalidTokenException:
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    break;
                case RevokeTokenBadRequest:
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    break;
                
                #endregion

                #region  Organization Exceptions

                case OrganizationNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case AccessLevelForbiddenException:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;

                #endregion

                #region Data Point Exceptions

                case DataPointKeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case DataPointNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case DashboardNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                #endregion
                
                
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }
            
            var result = JsonSerializer.Serialize(new { errorMessage = exception.Message });
            await response.WriteAsync(result);
            Console.WriteLine($"EXCEPTION: {exception.Message}\n\nSTACK TRACE:\n{exception.StackTrace}");
        }
    }
}

public static class ErrorHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandlerMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlerMiddleware>();
    } 
}