﻿using Application.JWT.Model;

namespace Application.JWT.Service;

public interface IJwtTokenGenerator
{
    public (string, DateTime) GenerateToken(Domain.DomainEntities.User user);
    public string ValidateToken(string? token);
    public JwtRefreshToken GenerateRefreshToken(string ipAddress);
}