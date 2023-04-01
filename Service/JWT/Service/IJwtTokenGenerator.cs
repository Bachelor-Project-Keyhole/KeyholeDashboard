using Service.JWT.Model;

namespace Service.JWT.Service;

public interface IJwtTokenGenerator
{
    public (string, DateTime) GenerateToken(Domain.DomainEntities.User user);
    public string ValidateToken(string? token);
    public JwtRefreshToken GenerateRefreshToken(string ipAddress);
}