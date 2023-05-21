// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618
namespace Application.JWT.Model;

public class AuthenticationResponse
{
    public string  Token { get; set; }
    public DateTime Expiration { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public UserAuthenticationResponse User { get; set; }
}

public class UserAuthenticationResponse
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string[] Roles { get; set; } 
    public string Name { get; set; }
    
}