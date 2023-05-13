using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

#pragma warning disable CS8618

namespace Domain.User;

public class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string? OwnedOrganizationId { get; set; }    // If is organization owner, store the id. (need to discuss if storing id is enough)
    public string? MemberOfOrganizationId { get; set; } // Store organization id of which user is part of.
    
    /* Should decide if these field are required. */
    public string FullName { get; set; }

    public DateTime RegistrationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    
    public List<UserAccessLevel> AccessLevels { get; set; } // Admin should also have viewer and editor role.
    
    [JsonIgnore]
    public string? PasswordHash { get; set; }

    public List<RefreshToken>? RefreshTokens { get; set; }
}

public class RefreshToken
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


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserAccessLevel
{
    Viewer,
    Editor,
    Admin
}