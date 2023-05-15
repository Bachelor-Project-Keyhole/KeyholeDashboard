using Application.JWT.Model;

namespace Application.Authentication.AuthenticationService;

public interface IUserAuthenticationService
{
    Task<AuthenticationResponse> Authenticate(AuthenticateRequest model);
    Task<AuthenticationResponse> RefreshToken(string? token);
    Task RevokeToken(string token);
    
}