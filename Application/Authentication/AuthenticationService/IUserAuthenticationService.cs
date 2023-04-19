using Application.JWT.Model;
using Microsoft.AspNetCore.Mvc;

namespace Application.Authentication.AuthenticationService;

public interface IUserAuthenticationService
{
    Task<AuthenticationResponse> Authenticate(AuthenticateRequest model);
    Task RefreshToken(string token);
    Task RevokeToken(string token);
    
}