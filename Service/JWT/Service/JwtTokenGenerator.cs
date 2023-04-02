using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.JWT.Model;

namespace Service.JWT.Service;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
    
    public (string, DateTime) GenerateToken(Domain.DomainEntities.User user)
    {
        // create claims and add user Access Levels
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        foreach (var role in user.AccessLevels)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }
        
        var expiration = DateTime.UtcNow.AddMinutes(30); // JWT token lifespan
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiration);
    }

    public string ValidateToken(string? token)
    {
        if (token == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                // ClockSkew set to zero, makes token expiration time more accurate.
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken) validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == "id").Value;
            return userId;
        }
        catch (Exception e)
        {
            // on validation fail. 
            return null;
        }
    }

    public JwtRefreshToken GenerateRefreshToken(string ipAddress)
    {
        var refreshToken = new JwtRefreshToken
        {
            Token = GenerateUniqueToken(),
            ExpirationTime = DateTime.UtcNow.AddDays(7),
            CreationTime = DateTime.UtcNow,
            CreatedByIpAddress = ipAddress
        };

        return refreshToken;
    }

    private string GenerateUniqueToken()
    {
        // token is a cryptographically strong random sequence of values
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
 
    }
}