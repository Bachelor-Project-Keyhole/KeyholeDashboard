using System.Text.Json.Serialization;
using Domain.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverQueried.Global
#pragma warning disable CS8618

namespace Repository.User.UserPersistence;

[BsonCollection("user")]
public class UserPersistenceModel : Document
{
    // TODO: find all usages where id is being send, instead of being generated here
    public string Email { get; set; }
    public string? OwnedOrganizationId { get; set; }    // If is organization owner, store the id. (need to discuss if storing id is enough)
    public string? MemberOfOrganizationId { get; set; } // Store organization id of which user is part of.
    
    /* Should decide if these field are required. */
    public string FullName { get; set; }
    
    /* Should decide if these field are required. */
    
    public DateTime RegistrationDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    [BsonRepresentation(BsonType.String)]
    public List<UserAccessLevel> AccessLevels { get; set; } // Admin should also have viewer and editor role.
    
    [JsonIgnore]
    public string? PasswordHash { get; set; }

    public List<PersistenceRefreshToken>? RefreshTokens { get; set; }
}

public class PersistenceRefreshToken
{
    public string Id { get; set; }
    public string Token { get; set; }                               // Refresh Token
    public DateTime ExpirationTime { get; set; }                    // Expiration date
    public DateTime CreationTime { get; set; }                      // Date of token creation
    public string CreatedByIpAddress { get; set; }                  // Ip on which token was created
    public DateTime? Revoked { get; set; }                          // Time when token got revoked
    public string? RevokedByIpAddress { get; set; }                  // Ip on which token was revoked
    public string? ReplacementToken { get; set; }                    // If token was replaced due to rotation
    public string? ReasonOfRevoke { get; set; }                      // Why token got revoked
    private bool IsExpired => DateTime.UtcNow >= ExpirationTime;    // did the token expired due to TTL (Time-To-Live) time
    public bool IsRevoked => Revoked != null;                       // Was token revoked ?
    public bool IsActive => !IsRevoked && !IsExpired;               // Is token still active.
}