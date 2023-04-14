using Application.JWT.Model;
using Application.JWT.Service;
using Application.User.UserService;
using AutoMapper;
using Domain.DomainEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Repository.User.UserRepository;

namespace Application.Authentication.AuthenticationService;

public class UserAuthenticationService : IUserAuthenticationService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserAuthenticationService(
        IJwtTokenGenerator tokenGenerator,
        IUserService userService,
        IHttpContextAccessor contextAccessor,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings)
    {
        _userService = userService;
        _tokenGenerator = tokenGenerator;
        _contextAccessor = contextAccessor;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
    }
    public async Task<AuthenticationResponse> Authenticate(AuthenticateRequest model)
    {
        var user = await _userService.GetUserByEmail(model.Email);
        if (user == null)
            throw new Exception(); // TODO: Fix exception
        var (tokenInfo, jwtExpiration) = _tokenGenerator.GenerateToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken(GetIpAddress(_contextAccessor.HttpContext)); // Not sure if it is possible to have in-active context at this point.

        var refreshTokenFromDb = _mapper.Map<RefreshToken>(user);
        
        if (!(user.RefreshTokens?.Count > 0))
            user.RefreshTokens = new List<RefreshToken> {refreshTokenFromDb};
        else
            user.RefreshTokens.Add(refreshTokenFromDb);

        RemoveOldRefreshTokens(user);

        await _userService.UpdateUser(user);
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

    public async Task RefreshToken(string token)
    {
        var user = await _userService.GetByRefreshToken(token);
        if(user == null)
            throw new ApplicationException("User missing"); // TODO: Fix exception

        var userRefreshToken = user.RefreshTokens?.Single(x => x.Token == token);
        if (userRefreshToken != null && userRefreshToken.IsRevoked)
        {
            // Refresh token was compromised and all user's refresh token should be revoked
            RevokeAllRefreshTokens(userRefreshToken, user, GetIpAddress(_contextAccessor.HttpContext!),
                $"Attempted reuse of revoked ancestor token: {token}");
        }

        await _userService.UpdateUser(user);
    }

    public async Task RevokeToken(string token)
    {
        var user = await _userService.GetByRefreshToken(token);
        if(user == null)
            throw new ApplicationException("User missing"); // TODO: Fix exception
        
        var userRefreshToken = user.RefreshTokens?.Single(x => x.Token == token);
        if (userRefreshToken == null || !userRefreshToken.IsActive)
            throw new ApplicationException("Invalid token");
        
       RevokeRefreshToken(userRefreshToken, GetIpAddress(_contextAccessor.HttpContext!), "Revoked without replacement"); 
    }

    private void RevokeAllRefreshTokens(RefreshToken refreshToken, Domain.DomainEntities.User user, string ipAddress, string reason)
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
    
    private void RemoveOldRefreshTokens(Domain.DomainEntities.User user)
    {
        // remove old inactive refresh tokens from user based on TTL in app settings
        user?.RefreshTokens?.RemoveAll(x => 
            !x.IsActive && 
            x.CreationTime.AddDays(_jwtSettings.RefreshTokenTtl) <= DateTime.UtcNow);
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

    private string GetIpAddress(HttpContext httpContext)
    {
        // Get source ip address for the current request
        if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            return httpContext.Request.Headers["X-Forwarded-For"]!;
        return httpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
    }
}