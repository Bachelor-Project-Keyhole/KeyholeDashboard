using Application.JWT.Helper;
using Application.JWT.Model;
using Application.JWT.Service;
using AutoMapper;
using Contracts.v1.Authentication;
using Domain;
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

    public UserAuthenticationService(
        IJwtTokenGenerator tokenGenerator,
        IUserRepository userRepository,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
    }
    public async Task<AuthenticationResponse> Authenticate(AuthenticateRequest model)
    {
        var user = await _userRepository.GetUserByEmail(model.Email);
        if (user == null)
            throw new UserNotFoundException("User by given email was not found");
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

        await _userRepository.UpdateUser(user);
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

    public async Task<AuthenticationResponse> RefreshToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            throw new InvalidTokenException("Token was not found in cookies");
        
        var user = await _userRepository.GetByRefreshToken(token);
        if (user == null)
            throw new UserNotFoundException("User with given token was not found");

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
            throw new UserNotFoundException("User missing");
        
        var userRefreshToken = user.RefreshTokens?.Single(x => x.Token == token);
        if (userRefreshToken == null || !userRefreshToken.IsActive || userRefreshToken.ExpirationTime <= DateTime.UtcNow)
            throw new InvalidTokenException("Invalid/Expired token");
        
        RevokeRefreshToken(userRefreshToken, "Revoked without replacement");
        await _userRepository.UpdateUser(user);
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