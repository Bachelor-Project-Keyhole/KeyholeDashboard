// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618
namespace Service.JWT.Model;

public class JwtRefreshToken
{
    public string Id { get; set; }
    public string Token { get; set; }                               // Refresh Token
    public DateTime ExpirationTime { get; set; }                    // Expiration date
    public DateTime CreationTime { get; set; }                      // Date of token creation
    public string CreatedByIpAddress { get; set; }                  // Ip on which token was created
    public DateTime? Revoked { get; set; }                          // Time when token got revoked
    public string RevokedByIpAddress { get; set; }                  // Ip on which token was revoked
    public string ReplacementToken { get; set; }                    // If token was replaced due to rotation
    public string ReasonOfRevoke { get; set; }                      // Why token got revoked
    private bool IsExpired => DateTime.UtcNow >= ExpirationTime;    // did the token expired due to TTL (Time-To-Live) time
    public bool IsRevoked => Revoked != null;                       // Was token revoked ?
    public bool IsActive => !IsRevoked && !IsExpired;               // Is token still active.
}