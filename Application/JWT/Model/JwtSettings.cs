#pragma warning disable CS8618
namespace Application.JWT.Model;

public class JwtSettings
{
    public string Secret { get; set; }
    
    /// <summary>
    /// Refresh token time to live (in days)
    /// Inactive tokens should be automatically deleted from db after TTL expiration
    /// </summary>
    public double RefreshTokenTtl { get; set; }
}