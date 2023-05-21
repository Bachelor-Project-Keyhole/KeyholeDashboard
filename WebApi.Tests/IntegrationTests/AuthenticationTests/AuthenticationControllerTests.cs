using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Application.JWT.Helper;
using Application.JWT.Model;
using Domain;
using Domain.User;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.User.UserPersistence;

namespace WebApi.Tests.IntegrationTests.AuthenticationTests;

public class AuthenticationControllerTests : IntegrationTest
{
    private async Task<UserPersistenceModel> SetupUser(string password, UserAccessLevel[] accessLevels)
    {
        var userPersistenceModel = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "auth@auth.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "646791352d33a03d8d495c2e",
            FullName = "Yo lama1",
            PasswordHash = PasswordHelper.GetHashedPassword(password), // Has to be at least 8 chars
            AccessLevels = accessLevels.ToList(),
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistenceModel });
        return userPersistenceModel;
    }

    [Theory]
    [InlineData(new[] { UserAccessLevel.Viewer, UserAccessLevel.Admin, UserAccessLevel.Editor })]
    [InlineData(new[] { UserAccessLevel.Editor })]
    [InlineData(new[] { UserAccessLevel.Viewer })]
    [InlineData(new[] { UserAccessLevel.Admin })]
    public async Task Authenticate_Successful_ForAccessLevels(UserAccessLevel[] accessLevels)
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = await SetupUser(password, accessLevels);

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };

        // Act
        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());

        // Assert
        responseAuth?.User.Id.Should().Be(userPersistenceModel.Id.ToString());
        responseAuth?.User.Email.Should().Be(userPersistenceModel.Email);
        responseAuth?.User.Roles
            .Should().BeEquivalentTo(userPersistenceModel.AccessLevels.Select(al => al.ToString()));
        responseAuth?.User.Name.Should().Be(userPersistenceModel.FullName);
        var userList = await GetAll<UserPersistenceModel>();
        var user = userList.FirstOrDefault(x => x.Id == userPersistenceModel.Id);
        user?.RefreshTokens.Should().NotBeNull();
    }

    [Fact]
    public async Task Authenticate_UserByEmailNotFound()
    {
        // Arrange
        var password = "password";
        await SetupUser(password, new[] { UserAccessLevel.Viewer });

        var auth = new AuthenticateRequest
        {
            Email = "random@random.YoLama",
            Password = password
        };

        // Act
        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);

        //TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        // Assert
        httpResponseMessageAuth.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var userList = await GetAll<UserPersistenceModel>();
        userList.Should().HaveCount(1);
    }

    [Fact]
    public async Task Authenticate_PasswordIncorrect()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = await SetupUser(password, new[] { UserAccessLevel.Viewer });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = "Nooope!"
        };

        // Act
        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);

        //TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        // Assert
        httpResponseMessageAuth.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var userList = await GetAll<UserPersistenceModel>();
        userList.Should().HaveCount(1);
    }

    [Fact]
    public async Task RefreshToken_NonCookie_Successful()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = await SetupUser(password, new[] { UserAccessLevel.Viewer });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };

        //Act
        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);

        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new AddNonRefreshTokenRequest
        {
            Token = responseAuth!.RefreshToken
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/token/refresh/cookie", UriKind.Relative),
                stringContent);

        //Assert
        var response =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessage.Content
                .ReadAsStringAsync());
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        response?.RefreshToken.Should().NotBe(request.Token);
        var users = await GetAll<UserPersistenceModel>();
        var user = users.FirstOrDefault(x => x.Email == auth.Email);
        user?.RefreshTokens?.FirstOrDefault(x => x.Token == response?.RefreshToken).Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshToken_NonCookie_InvalidToken()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = await SetupUser(password, new[] { UserAccessLevel.Viewer });


        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };

        // Act
        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");
        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri("api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new AddNonRefreshTokenRequest
        {
            Token = ""
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/token/refresh/cookie", UriKind.Relative),
                stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var users = await GetAll<UserPersistenceModel>();
        var user = users.FirstOrDefault(x => x.Email == auth.Email);
        user?.RefreshTokens?.Count.Should().Be(1);
    }

    [Fact]
    public async Task RefreshToken_NonCookie_TokenUserNotFound()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = await SetupUser(password, new[] { UserAccessLevel.Viewer });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };

        // Act
        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new AddNonRefreshTokenRequest
        {
            Token = "SomeRandomToken"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/token/refresh/cookie", UriKind.Relative),
                stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var users = await GetAll<UserPersistenceModel>();
        var user = users.FirstOrDefault(x => x.Email == auth.Email);
        user?.RefreshTokens?.Count.Should().Be(1);
    }

    [Fact]
    public async Task RefreshToken_NonCookie_TokenExpired()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "auth@auth.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "646791352d33a03d8d495c2e",
            FullName = "Yo lama1",
            PasswordHash = PasswordHelper.GetHashedPassword(password), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer },
            RefreshTokens = new List<PersistenceRefreshToken>()
            {
                new()
                {
                    Id = IdGenerator.GenerateId(),
                    Revoked = DateTime.UtcNow,
                    Token = "R7Agwhz1vEe6J/C4srV3Jb5sD0C89EJhIE9ullQLCTXknex104oVs3T2fpse2BA3MRWu0b36Xnc00ek9yxhaJA==",
                    CreationTime = DateTime.UtcNow,
                    ExpirationTime = DateTime.UtcNow,
                    ReplacementToken = null,
                    ReasonOfRevoke = "",
                    CreatedByIpAddress = "0.0.0.1",
                    RevokedByIpAddress = "0.0.0.1"
                }
            },
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistenceModel });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };

        // Act 
        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new AddNonRefreshTokenRequest
        {
            Token = "R7Agwhz1vEe6J/C4srV3Jb5sD0C89EJhIE9ullQLCTXknex104oVs3T2fpse2BA3MRWu0b36Xnc00ek9yxhaJA=="
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/token/refresh/cookie", UriKind.Relative),
                stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var users = await GetAll<UserPersistenceModel>();
        var user = users.FirstOrDefault(x => x.Email == auth.Email);
        user?.RefreshTokens?.Count.Should().Be(2);
    }

    [Fact]
    public async Task RevokeToken_Success()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = await SetupUser(password, new[] { UserAccessLevel.Viewer });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };

        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new LogoutRequest
        {
            Token = responseAuth!.RefreshToken
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/logout", UriKind.Relative), stringContent);

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Email == auth.Email);

        var refreshToken = user?.RefreshTokens!.SingleOrDefault(x => x.Token == responseAuth?.RefreshToken);

        refreshToken?.IsRevoked.Should().Be(true);
    }

    [Fact]
    public async Task RevokeToken_EmptyTokenBadRequest()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = await SetupUser(password, new[] { UserAccessLevel.Viewer });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };

        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new LogoutRequest
        {
            Token = ""
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/logout", UriKind.Relative), stringContent);

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var user = (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Email == auth.Email);
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeToken_UserByTokenNotFound()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = await SetupUser(password, new[] { UserAccessLevel.Viewer });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };
        
        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new LogoutRequest
        {
            Token = "SomeRandomToken"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/logout", UriKind.Relative), stringContent);

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var user = (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Email == auth.Email);
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeToken_ExpiredToken()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "auth@auth.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "646791352d33a03d8d495c2e",
            FullName = "Yo lama1",
            PasswordHash = PasswordHelper.GetHashedPassword(password), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer },
            RefreshTokens = new List<PersistenceRefreshToken>()
            {
                new()
                {
                    Id = IdGenerator.GenerateId(),
                    Revoked = DateTime.UtcNow,
                    Token = "R7Agwhz1vEe6J/C4srV3Jb5sD0C89EJhIE9ullQLCTXknex104oVs3T2fpse2BA3MRWu0b36Xnc00ek9yxhaJA==",
                    CreationTime = DateTime.UtcNow,
                    ExpirationTime = DateTime.UtcNow,
                    ReplacementToken = null,
                    ReasonOfRevoke = "",
                    CreatedByIpAddress = "0.0.0.1",
                    RevokedByIpAddress = "0.0.0.1"
                }
            },
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistenceModel });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };


        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new LogoutRequest
        {
            Token = "R7Agwhz1vEe6J/C4srV3Jb5sD0C89EJhIE9ullQLCTXknex104oVs3T2fpse2BA3MRWu0b36Xnc00ek9yxhaJA=="
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/logout", UriKind.Relative), stringContent);

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var user = (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Email == auth.Email);
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeToken_TTL()
    {
        // Arrange
        var password = "orange1234";
        var userPersistenceModel = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "auth@auth.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "646791352d33a03d8d495c2e",
            FullName = "Yo lama1",
            PasswordHash = PasswordHelper.GetHashedPassword(password), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer },
            RefreshTokens = new List<PersistenceRefreshToken>()
            {
                new()
                {
                    Id = IdGenerator.GenerateId(),
                    Revoked = DateTime.UtcNow,
                    Token = "R7Agwhz1vEe6J/C4srV3Jb5sD0C89EJhIE9ullQLCTXknex104oVs3T2fpse2BA3MRWu0b36Xnc00ek9yxhaJA==",
                    CreationTime = DateTime.UtcNow.AddDays(-2),
                    ExpirationTime = DateTime.UtcNow.AddDays(-1),
                    ReplacementToken = null,
                    ReasonOfRevoke = "",
                    CreatedByIpAddress = "0.0.0.1",
                    RevokedByIpAddress = "0.0.0.1"
                }
            },
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistenceModel });

        var auth = new AuthenticateRequest
        {
            Email = userPersistenceModel.Email,
            Password = password
        };


        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);

        var request = new LogoutRequest
        {
            Token = "R7Agwhz1vEe6J/C4srV3Jb5sD0C89EJhIE9ullQLCTXknex104oVs3T2fpse2BA3MRWu0b36Xnc00ek9yxhaJA=="
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/logout", UriKind.Relative), stringContent);

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var user = (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Email == auth.Email);
        user.Should().NotBeNull();
    }
}