using Application.JWT.Helper;
using Application.JWT.Model;
using Application.JWT.Service;
using AutoMapper;
using Contracts.v1.Authentication;
using Domain;
using Domain.Exceptions;
using Domain.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Authentication.AuthenticationService;

public class UserAuthenticationService : IUserAuthenticationService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserDomainService _userDomainService;
    private readonly IMapper _mapper;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public UserAuthenticationService(
        IJwtTokenGenerator tokenGenerator,
        IUserDomainService userDomainService,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings)
    {
        _userDomainService = userDomainService;
        _tokenGenerator = tokenGenerator;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
    }
    public async Task<AuthenticationResponse> Authenticate(AuthenticateRequest model)
    {
        var user = await _userDomainService.GetUserByEmail(model.Email);
        if (!PasswordHelper.ComparePasswords(model.Password, user.PasswordHash))
            throw new UserForbiddenAction("Incorrect credentials");
        
        var (tokenInfo, jwtExpiration) = _tokenGenerator.GenerateToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken(); // Not sure if it is possible to have in-active context at this point.

        var newRefreshToken = _mapper.Map<RefreshToken>(refreshToken);
        newRefreshToken.Id = IdGenerator.GenerateId();

        if (!(user.RefreshTokens?.Count > 0))
            user.RefreshTokens = new List<RefreshToken> {newRefreshToken};
        else
            user.RefreshTokens.Add(newRefreshToken);

        RemoveOldRefreshTokens(user);

        await _userDomainService.UpdateUser(user);
        return new AuthenticationResponse
        {
            Token = tokenInfo,
            Expiration = jwtExpiration,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpirationTime,
            User = new UserAuthenticationResponse
            {
                Id = user.Id,
                Email = user.Email,
                OrganizationId = user.MemberOfOrganizationId,
                Roles = user.AccessLevels
                    .Select(al => al.ToString())
                    .ToArray(),
                Name = user.FullName
            }
        };
    }

    public async Task<RefreshTokenRotateResponse> RefreshToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            throw new InvalidTokenException("Token was not found in cookies");
        
        var user = await _userDomainService.GetByRefreshToken(token);
        
        var userRefreshToken = user.RefreshTokens?.Single(x => x.Token == token);
        if (userRefreshToken != null && (userRefreshToken.IsRevoked || userRefreshToken.ExpirationTime <= DateTime.UtcNow))
        {
            // Refresh token was compromised and all user's refresh token should be revoked
            RevokeAllRefreshTokens(userRefreshToken, user,
                $"Attempted reuse of revoked ancestor token: {token}");
        }

        if (userRefreshToken != null && !userRefreshToken.IsActive)
            throw new InvalidTokenException("Invalid token");
        
        // Replace old refresh token to new one(rotate)
        var refreshToken = RotateRefreshToken(userRefreshToken!);
        user.RefreshTokens?.Add(refreshToken);
        
        RemoveOldRefreshTokens(user);
        await _userDomainService.UpdateUser(user);
        
        // generate new JWT token
        var (jwtToken, jwtExpiration) = _tokenGenerator.GenerateToken(user);
        return new RefreshTokenRotateResponse
        {
            Token = jwtToken,
            Expiration = jwtExpiration,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpirationTime
        };
    }

    public async Task RevokeToken(string token)
    {
        var user = await _userDomainService.GetByRefreshToken(token);
        var userRefreshToken = user.RefreshTokens?.Single(x => x.Token == token);
        if (userRefreshToken == null || !userRefreshToken.IsActive || userRefreshToken.ExpirationTime <= DateTime.UtcNow)
            throw new InvalidTokenException("Invalid/Expired token");
        
        RevokeRefreshToken(userRefreshToken, "Revoked without replacement");
        await _userDomainService.UpdateUser(user);
    }

    private void RevokeAllRefreshTokens(RefreshToken refreshToken, Domain.User.User user, string reason)
    {
        if (!string.IsNullOrEmpty(refreshToken.ReplacementToken))
        {
            var childToken = user.RefreshTokens?.SingleOrDefault(x => x.Token == refreshToken.ReplacementToken);
            if (childToken != null && childToken.IsActive)
                RevokeRefreshToken(childToken, reason);
            else
                RevokeAllRefreshTokens(childToken!, user, reason);
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

    private RefreshToken RotateRefreshToken(RefreshToken refreshToken)
    {
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();
        RevokeRefreshToken(refreshToken, "Replaced by new token", newRefreshToken.Token);
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

    // private string GetIpAddress(HttpContext? httpContext)
    // {
    //     // Get source ip address for the current request
    //     if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
    //         return httpContext.Request.Headers["X-Forwarded-For"]!;
    //     return httpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
    // }
}