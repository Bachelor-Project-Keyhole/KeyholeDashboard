// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618
namespace Service.JWT.Model;

public class AuthenticationResponse
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string? OwnedOrganizationId { get; set; }
    public string? MemberOfOrganizationId { get; set; }
    public string JwtToken { get; set; }
    public string RefreshToken { get; set; }

    public AuthenticationResponse(Domain.DomainEntities.User user, string jwtToken, string refreshToken)
    {
        Id = user.Id.ToString();
        Email = user.Email;
        FullName = user.FullName;
        OwnedOrganizationId = user.OwnedOrganizationId;
        MemberOfOrganizationId = user.MemberOfOrganizationId;
        JwtToken = jwtToken;
        RefreshToken = refreshToken;
    }
}