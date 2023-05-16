using Application.JWT.Model;
using Domain.User;

namespace Application.JWT.Service;

public interface IJwtTokenGenerator
{
    public (string, DateTime) GenerateToken(Domain.User.User user);
    public TokenValidationModel? ValidateToken(string? token);
    public RefreshToken GenerateRefreshToken(string ipAddress);
}