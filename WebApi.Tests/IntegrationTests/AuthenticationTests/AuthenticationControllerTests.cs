using System.Net;
using Application.Authentication.AuthenticationService;
using Application.JWT.Model;
using Application.User.UserService;
using AutoMapper;
using Domain.User;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebApi.Controllers.V1.Authentication;

namespace WebApi.Tests.IntegrationTests.AuthenticationTests;

public class AuthenticationControllerTests : IntegrationTest
{
    
    
    [Fact]
    public async Task Authenticate_Successful()
    {
        
        var mockAuthenticationService = Substitute.For<IUserAuthenticationService>();
        var mockUserService = Substitute.For<IUserService>();
        var mockMapper = Substitute.For<IMapper>();
        var controller = new AuthenticationController(mockAuthenticationService, mockMapper, mockUserService);
        var request = new AuthenticateRequest { Email = "test@test.com", Password = "testpassword" };
        var token = "testtoken";
        var refreshToken = "testrefreshtoken";
        var user = new User { Id = "1", Email = "test@test.com" };
        var authResponse = new AuthenticationResponse
        {
            Token = token,
            Expiration = DateTime.Now.AddMinutes(10),
            RefreshToken = refreshToken,
            RefreshTokenExpiration = DateTime.Now.AddDays(7),
            User = new UserAuthenticationResponse
            {
                Id = user.Id,
                Email = user.Email
            }
        };
        mockAuthenticationService.Authenticate(request).Returns(authResponse);
    
        // Act
        var result = await controller.Authenticate(request) as ObjectResult;
        var actualResult = result?.Value as AuthenticationResponse;
    
        // Assert
        result.Should().NotBeNull();
        result?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        actualResult.Should().BeEquivalentTo(authResponse);
    }
   
}