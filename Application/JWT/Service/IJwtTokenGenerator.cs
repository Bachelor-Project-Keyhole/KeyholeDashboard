using Application.JWT.Model;

namespace Application.JWT.Service;

public interface IJwtTokenGenerator
{
    public (string, DateTime) GenerateToken(Domain.User.User user);
    public TokenValidationModel? ValidateToken(string? token);
    public JwtRefreshToken GenerateRefreshToken(string ipAddress);
}