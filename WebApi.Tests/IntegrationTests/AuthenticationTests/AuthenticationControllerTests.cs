using System.Net;
using System.Text;
using Application.Authentication.AuthenticationService;
using Application.JWT.Model;
using Application.JWT.Service;
using Application.User.UserService;
using AutoMapper;
using Domain.User;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using NSubstitute;
using Repository.User.UserPersistence;
using WebApi.Controllers.V1.Authentication;

namespace WebApi.Tests.IntegrationTests.AuthenticationTests;

public class AuthenticationControllerTests : IntegrationTest
{
    
    
    // [Fact]
    // public async Task Authenticate_Successful()
    // {
    //     
    //     var mockAuthenticationService = Substitute.For<IUserAuthenticationService>();
    //     var mockUserService = Substitute.For<IUserService>();
    //     var mockMapper = Substitute.For<IMapper>();
    //     var controller = new AuthenticationController(mockAuthenticationService, mockMapper, mockUserService);
    //     var request = new AuthenticateRequest { Email = "test@test.com", Password = "testpassword" };
    //     var token = "testtoken";
    //     var refreshToken = "testrefreshtoken";
    //     var user = new User { Id = "1", Email = "test@test.com" };
    //     var authResponse = new AuthenticationResponse
    //     {
    //         Token = token,
    //         Expiration = DateTime.Now.AddMinutes(10),
    //         RefreshToken = refreshToken,
    //         RefreshTokenExpiration = DateTime.Now.AddDays(7),
    //         User = new UserAuthenticationResponse
    //         {
    //             Id = user.Id,
    //             Email = user.Email
    //         }
    //     };
    //     mockAuthenticationService.Authenticate(request).Returns(authResponse);
    //
    //     // Act
    //     var result = await controller.Authenticate(request) as ObjectResult;
    //     var actualResult = result?.Value as AuthenticationResponse;
    //
    //     // Assert
    //     result.Should().NotBeNull();
    //     result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    //     actualResult.Should().BeEquivalentTo(authResponse);
    // }
    //
    // [Fact]
    // public async Task Authenticate_CredentialsInvalid()
    // {
    //     // Arrange
    //     var mockAuthenticationService = Substitute.For<IUserAuthenticationService>();
    //     var mockUserService = Substitute.For<IUserService>();
    //     var mockMapper = Substitute.For<IMapper>();
    //     var controller = new AuthenticationController(mockAuthenticationService, mockMapper, mockUserService);
    //     var request = new AuthenticateRequest { Email = "test@test.com", Password = "testpassword" };
    //     var token = "testtoken";
    //     var refreshToken = "testrefreshtoken";
    //     var user = new User { Id = "1", Email = "test@test.com" };
    //
    //     var userPersistence = new UserPersistenceModel();
    //     
    //     var authResponse = new AuthenticationResponse
    //     {
    //         Token = token,
    //         Expiration = DateTime.Now.AddMinutes(10),
    //         RefreshToken = refreshToken,
    //         RefreshTokenExpiration = DateTime.Now.AddDays(7),
    //         User = new UserAuthenticationResponse
    //         {
    //             Id = user.Id,
    //             Email = user.Email
    //         }
    //     };
    //
    //     mockAuthenticationService.Authenticate(request).Returns(authResponse);
    //
    //     // Expect an exception to be thrown when the user is not found
    //     mockUserService.GetUserByEmail(request.Email)!.Returns(Task.FromResult<User>(null));
    //
    //     // Act
    //     Func<Task> act = async () => { await controller.Authenticate(request); };
    //
    //     // Assert
    //     await act.Should().ThrowAsync<Exception>();
    // }
    private void Setup()
    {
        
    }

    [Fact]
    public async Task Authenticate_ValidCredentials_ReturnsAuthenticationResponse()
    {
        
        
        // Arrange
        var mockAuthenticationService = Substitute.For<IUserAuthenticationService>();
        var mockUserService = Substitute.For<IUserService>();
        var mockMapper = Substitute.For<IMapper>();
        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var httpAccessor = Substitute.For<IHttpContextAccessor>();
        var controller = new AuthenticationController(mockAuthenticationService, mockMapper, mockUserService);
        httpAccessor.HttpContext = new DefaultHttpContext();
        httpAccessor.HttpContext.Request.Headers["X-Forwarded-For"] = "127.0.0.1"; // set the required header
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer test-token";

        var password = "orange";
        var user = new UserPersistenceModel
        {
            Id = ObjectId.GenerateNewId(),
            Email = "test@test.com",
            AccessLevels = new List<UserAccessLevel>
                {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
            FullName = "test",
            PasswordHash = Application.JWT.Helper.PasswordHelper.GetHashedPassword(password),
            RefreshTokens = new List<PersistenceRefreshToken>(),
            OwnedOrganizationId = null,
            MemberOfOrganizationId = null,
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };
        
        var token = "jJ2q1uu7E29zwpFJ4Kr1NFCRVP/r1c81LkYYHxFfiAVTyhtacM0vTbnYX7/tA+YEZRLayg9tM5jBjgrQXBkefA==";
        var refreshToken = new PersistenceRefreshToken() 
        { 
            Id = "1",
            Token = token,
            ExpirationTime = DateTime.UtcNow.AddMinutes(30),
            CreationTime = DateTime.UtcNow,
            CreatedByIpAddress = "0.0.0.1",
            Revoked = null,
            ReplacementToken = null,
            ReasonOfRevoke = null,
            RevokedByIpAddress = null
        };
        
        var response = new AuthenticationResponse
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(30),
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpirationTime,
            User = new UserAuthenticationResponse { Id = user.Id.ToString(), Email = user.Email }
        };
        
        mockAuthenticationService.Authenticate(Arg.Any<AuthenticateRequest>()).Returns(response);

        
        
        user.RefreshTokens.Add(refreshToken);
        await PopulateDatabase(new []{user});
        
        // var response = new AuthenticationResponse
        // {
        //     Token = token,
        //     Expiration = DateTime.UtcNow.AddMinutes(30),
        //     RefreshToken = refreshToken.Token,
        //     RefreshTokenExpiration = refreshToken.ExpirationTime,
        //     User = new UserAuthenticationResponse {Id = user.Id.ToString(), Email = user.Email}
        // };
        
        // mockAuthenticationService.Authenticate(Arg.Any<AuthenticateRequest>()).Returns(response);

        
        var request = new AuthenticateRequest { Email = "test@test.com", Password = "orange" };
        
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        
        
        // Act
        var httpResponseMessage = 
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login",UriKind.Relative), stringContent);

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

    }
    
}