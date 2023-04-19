using Application.JWT.Model;

namespace Application.Authentication.AuthenticationService;

public interface IUserAuthenticationService
{
    Task<AuthenticationResponse> Authenticate(AuthenticateRequest model);
    Task RefreshToken(string token);
    Task RevokeToken(string token);
    
}