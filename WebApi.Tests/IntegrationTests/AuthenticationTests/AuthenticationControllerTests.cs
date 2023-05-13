using System.Net;
using System.Text;
using Application.Authentication.AuthenticationService;
using Application.User.UserService;
using AutoMapper;
using Domain;
using Domain.User;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using WebApi.Controllers.V1.Authentication;
using Moq;

namespace WebApi.Tests.IntegrationTests.AuthenticationTests;

public class AuthenticationControllerTests : IntegrationTest
{
    
    
//     [Fact]
//     public async Task Authenticate_Successful()
//     {
//         
//         var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
//         var mockHttpContext = new Mock<HttpContext>();
//
//         mockHttpContext.Setup(m => m.Request.Scheme).Returns("https");
//         mockHttpContext.Setup(m => m.Request.Host).Returns(new HostString("localhost"));
//         mockHttpContext.Setup(m => m.Request.Path).Returns("/your-endpoint");
//
//         mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);
//         
//         
//         var mockAuthService = new Mock<IUserAuthenticationService>();
//         var mockMapper = new Mock<IMapper>();
//         var mockUserService = new Mock<IUserService>();
//
//         var controllerMock = new AuthenticationController(
//             mockAuthService.Object,
//             mockMapper.Object,
//             mockUserService.Object);
//         
//         // Arrange
//         var password = "orange";
//         var userEntity = new Repository.User.UserPersistence.UserPersistenceModel
//         {
//             Id = ObjectId.Parse(IdGenerator.GenerateId()),
//             Email = "test@test.test",
//             FullName = "test",
//             AccessLevels = new List<UserAccessLevel>
//                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
//             PasswordHash = Application.JWT.Helper.PasswordHelper.GetHashedPassword(password),
//             RefreshTokens = new List<Repository.User.UserPersistence.PersistenceRefreshToken>(),
//             MemberOfOrganizationId = null,
//             ModifiedDate = DateTime.UtcNow,
//             OwnedOrganizationId = null,
//             RegistrationDate = DateTime.UtcNow,
//         };
//         
//         // Create a new instance of the controller
//         var controller = controllerMock;
//
//         // Create a new instance of HttpContext
//         var httpContext = new DefaultHttpContext();
//
//         // Set up the HttpContext with required properties
//         httpContext.Request.Scheme = "https";
//         httpContext.Request.Host = new HostString("localhost");
//         httpContext.Request.Path = "/your-endpoint";
//         httpContext.Response.Body = new MemoryStream();
//
// // Set the HttpContext of the controller's ControllerContext
//         controller.ControllerContext = new ControllerContext()
//         {
//             HttpContext = httpContext
//         };
//         
//         await PopulateDatabase(new[]{userEntity});
//         
//         var credentials = new Application.JWT.Model.AuthenticateRequest
//         {
//             Email = userEntity.Email,
//             Password = password
//         };
//         
//         var stringContent = 
//             new StringContent(JsonConvert.SerializeObject(credentials), Encoding.UTF8, "application/json");
//         
//         
//         // Act
//         var httpResponseMessage =
//             await TestClient.PostAsync(new Uri($"/api/v1/Authentication/login", UriKind.Relative), stringContent);
//         
//         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
//
//     }
}