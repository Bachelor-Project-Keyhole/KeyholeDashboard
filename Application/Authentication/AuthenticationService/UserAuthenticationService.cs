﻿using Application.JWT.Model;
using Application.JWT.Service;
using AutoMapper;
using Domain.Exceptions;
using Domain.RepositoryInterfaces;
using Domain.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Authentication.AuthenticationService;

public class UserAuthenticationService : IUserAuthenticationService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserAuthenticationService(
        IJwtTokenGenerator tokenGenerator,
        IUserRepository userRepository,
        IHttpContextAccessor contextAccessor,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _contextAccessor = contextAccessor;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
    }
    public async Task<AuthenticationResponse> Authenticate(AuthenticateRequest model)
    {
        var httpContext = _contextAccessor.HttpContext;
        var user = await _userRepository.GetUserByEmail(model.Email);
        if (user == null)
            throw new Exception(); // TODO: Fix exception
        var (tokenInfo, jwtExpiration) = _tokenGenerator.GenerateToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken(GetIpAddress(httpContext)); // Not sure if it is possible to have in-active context at this point.

        var refreshTokenFromDb = _mapper.Map<RefreshToken>(refreshToken);

        if (!(user.RefreshTokens?.Count > 0))
            user.RefreshTokens = new List<RefreshToken> {refreshTokenFromDb};
        else
            user.RefreshTokens.Add(refreshTokenFromDb);

        RemoveOldRefreshTokens(user);

        await _userRepository.UpdateUser(user);
        return new AuthenticationResponse
        {
            Token = tokenInfo,
            Expiration = jwtExpiration,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpirationTime,
            User = new UserAuthenticationResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email
            }
        };
    }

    public async Task<AuthenticationResponse> RefreshToken(string? token)
    {
        if (token == null)
            throw new InvalidTokenException("Token was not found in cookies");
        
        var user = await _userRepository.GetByRefreshToken(token);
        if (user == null)
            throw new UserNotFoundException("User with given token was not found");

        var userRefreshToken = user.RefreshTokens?.Single(x => x.Token == token);
        if (userRefreshToken != null && userRefreshToken.IsRevoked)
        {
            // Refresh token was compromised and all user's refresh token should be revoked
            RevokeAllRefreshTokens(userRefreshToken, user, GetIpAddress(_contextAccessor.HttpContext!),
                $"Attempted reuse of revoked ancestor token: {token}");
        }

        if (userRefreshToken != null && !userRefreshToken.IsActive)
            throw new InvalidTokenException("Invalid token");
        
        // Replace old refresh token to new one(rotate)
        var refreshToken = RotateRefreshToken(userRefreshToken!, GetIpAddress(_contextAccessor.HttpContext!));
        user.RefreshTokens?.Add(refreshToken);
        
        RemoveOldRefreshTokens(user);
        await _userRepository.UpdateUser(user);
        
        // generate new JWT token
        var (jwtToken, jwtExpiration) = _tokenGenerator.GenerateToken(user);
        return new AuthenticationResponse
        {
            Token = jwtToken,
            Expiration = jwtExpiration,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpirationTime,
            User = new UserAuthenticationResponse
            {
                Id = user.Id,
                Email = user.Email
            }
        };
    }

    public async Task RevokeToken(string token)
    {
        var user = await _userRepository.GetByRefreshToken(token);
        if(user == null)
            throw new ApplicationException("User missing"); // TODO: Fix exception
        
        var userRefreshToken = user.RefreshTokens?.Single(x => x.Token == token);
        if (userRefreshToken == null || !userRefreshToken.IsActive)
            throw new ApplicationException("Invalid token");
        
        RevokeRefreshToken(userRefreshToken, GetIpAddress(_contextAccessor.HttpContext!), "Revoked without replacement");
        await _userRepository.UpdateUser(user);
    }

    private void RevokeAllRefreshTokens(RefreshToken refreshToken, Domain.User.User user, string ipAddress, string reason)
    {
        if (!string.IsNullOrEmpty(refreshToken.ReplacementToken))
        {
            var childToken = user.RefreshTokens?.SingleOrDefault(x => x.Token == refreshToken.ReplacementToken);
            if (childToken != null && childToken.IsActive)
                RevokeRefreshToken(childToken, ipAddress, reason);
            else
                RevokeAllRefreshTokens(childToken!, user, ipAddress, reason);
        }
    }

    private void RevokeRefreshToken(RefreshToken token, string ipAddress, string reason = null!,
        string replacedByToken = null!)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIpAddress = ipAddress;
        token.ReasonOfRevoke = reason;
        token.ReplacementToken = replacedByToken;
    }
    
    private void RemoveOldRefreshTokens(Domain.User.User user)
    {
        // remove old inactive refresh tokens from user based on TTL in app settings
        user?.RefreshTokens?.RemoveAll(x => 
            !x.IsActive && 
            x.CreationTime.AddDays(_jwtSettings.RefreshTokenTtl) <= DateTime.UtcNow);
    }

    private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
    {
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void SetTokenCookie(HttpResponse response, string token)
    {
        // append cookie with refresh token to the http response
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private string GetIpAddress(HttpContext? httpContext)
    {
        // Get source ip address for the current request
        if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            return httpContext.Request.Headers["X-Forwarded-For"]!;
        return httpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
    }
}