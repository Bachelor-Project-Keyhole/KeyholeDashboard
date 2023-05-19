using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Application.JWT.Helper;
using Application.JWT.Model;
using Application.Organization.Model;
using Application.User.Model;
using Domain;
using Domain.User;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Organization;
using Repository.OrganizationUserInvite;
using Repository.User.UserPersistence;

namespace WebApi.Tests.IntegrationTests.OrganizationController;

public class OrganizationControllerTests : IntegrationTest
{

    [Fact]
    public async Task PostOrganizationAndUser_Successful()
    {
        // Arrange
        var request = new CreateAdminAndOrganizationRequest
        {
            Email = "test@testRequest.com",
            FullName = "Yo lama",
            OrganizationName = "OrgName11",
            Password = "orange1234"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage = await TestClient.PostAsync(new Uri("/api/v1/Organization/register", UriKind.Relative), stringContent);
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = JsonConvert.DeserializeObject<AdminAndOrganizationCreateResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
        var allOrganization = await GetAll<OrganizationPersistenceModel>();
        var allUser = await GetAll<UserPersistenceModel>();

        var organization = allOrganization.Single();
        organization.OrganizationName.Should().Be(request.OrganizationName);
        response?.OrganizationId.Should().Be(organization.Id.ToString());
        
        var user = allUser.Single();
        user.Id.ToString().Should().Be(response?.UserId);
        user.Email.Should().Be(request.Email);
        user.FullName.Should().Be(request.FullName);
        user.PasswordHash.Should().Be(PasswordHelper.GetHashedPassword("orange1234"));
    }

    [Fact]
    public async Task PostOrganizationAndUser_PasswordTooShort()
    {
        // Arrange
        var request = new CreateAdminAndOrganizationRequest
        {
            Email = "test@testRequest.com",
            FullName = "Yo lama",
            OrganizationName = "OrgName11",
            Password = "oran"
        };
        
        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage = await TestClient.PostAsync(new Uri("/api/v1/Organization/register", UriKind.Relative), stringContent);
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var allOrganization = await GetAll<OrganizationPersistenceModel>();
        var allUser = await GetAll<UserPersistenceModel>();
        allOrganization.Should().BeNullOrEmpty();
        allUser.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task PostOrganizationAndUser_EmailTaken()
    {
        // Arrange
        var userPersistence = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "test@test.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "",
            FullName = "Yo lama",
            PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };
        await PopulateDatabase(new[] {userPersistence});

        
        var request = new CreateAdminAndOrganizationRequest
        {
            Email = "test@test.com",
            FullName = "Yo lama",
            OrganizationName = "OrgName11",
            Password = "orange1223311"
        };
        
        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage = await TestClient.PostAsync(new Uri("/api/v1/Organization/register", UriKind.Relative), stringContent);
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var allOrganization = await GetAll<OrganizationPersistenceModel>();
        var allUser = await GetAll<UserPersistenceModel>();
        allOrganization.Should().BeNullOrEmpty();
        allUser.Should().HaveCount(1);
    }

    [Fact]
     public async Task InviteUser_Successful() // Change auth attribute to pass
     {
         // Arrange
         await Authenticate();
         var userPersistence = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test@test.com",
             OwnedOrganizationId = "",
             MemberOfOrganizationId = "",
             FullName = "Yo lama",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         

         var organizationPersistence = new OrganizationPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             OrganizationOwnerId = "",
             OrganizationName = "OrgName",
             CreationDate = DateTime.UtcNow,
             ModificationDate = DateTime.UtcNow
         };
    
         await PopulateDatabase(new[] {organizationPersistence});
         await PopulateDatabase(new[] {userPersistence});

         var request = new OrganizationUserInviteRequest
         {
             OrganizationId = organizationPersistence.Id.ToString(),
             UserId = userPersistence.Id.ToString(),
             AccessLevels = new List<UserAccessLevel> {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             ReceiverEmailAddress = "dziugis10@gmail.com",
         };
         
         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
    
         // Act
         var httpResponseMessage = await TestClient.PostAsync(new Uri("/api/v1/Organization/invite", UriKind.Relative), stringContent);
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    
     }

     [Fact]
     public async Task CompleteRegistrationByInvitation_Successful() // Change auth attribute to pass
     {
         // Arrange
         await Authenticate();
         var invitationOfUser = new OrganizationUserInvitePersistence
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             OrganizationId = IdGenerator.GenerateId(),
             Token = "5a6f8a",
             ReceiverEmail = "test@test.com",
             hasAccepted = false,
             TokenExpirationTime = DateTime.UtcNow.AddDays(2),
             RemoveFromDbDate = DateTime.UtcNow.AddDays(5),
             AccessLevels = new List<UserAccessLevel> {UserAccessLevel.Viewer, UserAccessLevel.Editor},
             InvitedByUserId = IdGenerator.GenerateId()
         };
             
         await PopulateDatabase(new[] {invitationOfUser});

         var request = new UserRegistrationRequest
         {
             FullName = "fName",
             Password = "orange1234"
         };
         
         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
    
         // Act
         var httpResponseMessage = await TestClient.PostAsync(new Uri($"/api/v1/Organization/register/{invitationOfUser.Token}", UriKind.Relative), stringContent);
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
         var response = JsonConvert.DeserializeObject<UserRegistrationResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
         response?.Email.Should().Be(invitationOfUser.ReceiverEmail);
         response?.FullName.Should().Be(request.FullName);
         response?.OrganizationId.Should().Be(invitationOfUser.OrganizationId);

     }

     [Fact]
     public async Task CompleteRegistrationByInvitation_InvalidToken() // Change auth attribute to pass
     {
         // Arrange
         await Authenticate();
         var invitationOfUser = new OrganizationUserInvitePersistence
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             OrganizationId = IdGenerator.GenerateId(),
             Token = "5a6f8a",
             ReceiverEmail = "test@test.com",
             hasAccepted = false,
             TokenExpirationTime = DateTime.UtcNow.AddDays(2),
             RemoveFromDbDate = DateTime.UtcNow.AddDays(5),
             AccessLevels = new List<UserAccessLevel> {UserAccessLevel.Viewer, UserAccessLevel.Editor},
             InvitedByUserId = IdGenerator.GenerateId()
         };
             
         await PopulateDatabase(new[] {invitationOfUser});

         var request = new UserRegistrationRequest
         {
             FullName = "fName",
             Password = "orange1234"
         };
         
         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
    
         // Act
         var httpResponseMessage = await TestClient.PostAsync(new Uri("/api/v1/Organization/register/aaaaaa", UriKind.Relative), stringContent);
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
     }
     
     [Fact]
     public async Task CompleteRegistrationByInvitation_ExpiredToken() // Change auth attribute to pass
     {
         // Arrange
         await Authenticate();
         var invitationOfUser = new OrganizationUserInvitePersistence
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             OrganizationId = IdGenerator.GenerateId(),
             Token = "5a6f8a",
             ReceiverEmail = "test@test.com",
             hasAccepted = false,
             TokenExpirationTime = DateTime.UtcNow.AddDays(-2),
             RemoveFromDbDate = DateTime.UtcNow.AddDays(1),
             AccessLevels = new List<UserAccessLevel> {UserAccessLevel.Viewer, UserAccessLevel.Editor},
             InvitedByUserId = IdGenerator.GenerateId()
         };
             
         await PopulateDatabase(new[] {invitationOfUser});

         var request = new UserRegistrationRequest
         {
             FullName = "fName",
             Password = "orange1234"
         };
         
         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
    
         // Act
         var httpResponseMessage = await TestClient.PostAsync(new Uri($"/api/v1/Organization/register/{invitationOfUser.Token}", UriKind.Relative), stringContent);
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
     }
     
     [Fact]
     public async Task CompleteRegistrationByInvitation_TokenAlreadyUsed() // Change auth attribute to pass
     {
         // Arrange
         await Authenticate();
         var invitationOfUser = new OrganizationUserInvitePersistence
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             OrganizationId = IdGenerator.GenerateId(),
             Token = "5a6f8a",
             ReceiverEmail = "test@test.com",
             hasAccepted = true,
             TokenExpirationTime = DateTime.UtcNow.AddDays(2),
             RemoveFromDbDate = DateTime.UtcNow.AddDays(1),
             AccessLevels = new List<UserAccessLevel> {UserAccessLevel.Viewer, UserAccessLevel.Editor},
             InvitedByUserId = IdGenerator.GenerateId()
         };
             
         await PopulateDatabase(new[] {invitationOfUser});

         var request = new UserRegistrationRequest
         {
             FullName = "fName",
             Password = "orange1234"
         };
         
         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
    
         // Act
         var httpResponseMessage = await TestClient.PostAsync(new Uri($"/api/v1/Organization/register/{invitationOfUser.Token}", UriKind.Relative), stringContent);
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
     }
     
     [Fact]
     public async Task GetAllUsersOfOrganization() // Change auth attribute to pass
     {
         
         
         // Arrange
         var orgId = IdGenerator.GenerateId();
         await Authenticate();
         var userPersistence1 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test1@test.com",
             OwnedOrganizationId = "",
             MemberOfOrganizationId = orgId,
             FullName = "Yo lama1",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         var userPersistence2 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test2@test.com",
             OwnedOrganizationId = "",
             MemberOfOrganizationId = orgId,
             FullName = "Yo lama2",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         
         await PopulateDatabase(new[] {userPersistence1});
         await PopulateDatabase(new[] {userPersistence2});


         var auth = new AuthenticateRequest
         {
             Email = userPersistence1.Email,
             Password = userPersistence1.PasswordHash
         };
         
         
         var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

         var httpResponseMessageAuth = await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative),stringContentAuth);
         var responseAuth = JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content.ReadAsStringAsync());


         TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);
         
         // Act
         var httpResponseMessage = await TestClient.GetAsync(new Uri($"api/v1/Organization/users/{orgId}", UriKind.Relative));
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
         
         var response = JsonConvert.DeserializeObject<AllUsersOfOrganizationResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
         response?.Users?.Count.Should().Be(2);
         response?.OrganizationId.Should().Be(orgId);
         
         var user1 = response?.Users?.Single(x => x.Email == userPersistence1.Email);
         var user2 = response?.Users?.Single(x => x.Email == userPersistence1.Email);
         response?.Users.Should().Contain(user1!);
         response?.Users.Should().Contain(user2!);
         
     }
     
     [Fact]
     public async Task UpdateOrganization_Successful() // Change auth attribute to pass
     {
         // Arrange
         await Authenticate();
         var organizationPersistence = new OrganizationPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             OrganizationOwnerId = "",
             OrganizationName = "OrgName",
             CreationDate = DateTime.UtcNow,
             ModificationDate = DateTime.UtcNow
         };
         
         await PopulateDatabase(new[] {organizationPersistence});

         var request = new UpdateOrganizationRequest
         {
             OrganizationName = "Changed Name",
             OrganzationId = organizationPersistence.Id.ToString()
         };
         
         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
    
         // Act
         var httpResponseMessage = await TestClient.PutAsync(new Uri($"/api/v1/Organization/update", UriKind.Relative), stringContent);

         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
         var response = JsonConvert.DeserializeObject<OrganizationResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
         response?.OrganizationName.Should().Be(request.OrganizationName);
     }
     
     [Fact]
     public async Task UpdateOrganization_NotFound() // Change auth attribute to pass
     {
         // Arrange
         await Authenticate();
         var organizationPersistence = new OrganizationPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             OrganizationOwnerId = "",
             OrganizationName = "OrgName",
             CreationDate = DateTime.UtcNow,
             ModificationDate = DateTime.UtcNow
         };
         
         await PopulateDatabase(new[] {organizationPersistence});

         var request = new UpdateOrganizationRequest
         {
             OrganizationName = "Changed Name",
             OrganzationId = IdGenerator.GenerateId()
         };
         
         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
    
         // Act
         var httpResponseMessage = await TestClient.PutAsync(new Uri($"/api/v1/Organization", UriKind.Relative), stringContent);

         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
         
     }

     [Fact]
     public async Task RemoveUserFromOrganization_Successful() // Change auth attribute to pass
     {
         // Arrange 
         var orgId = IdGenerator.GenerateId();
         await Authenticate();
         var userPersistence1 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test1@test.com",
             OwnedOrganizationId = null,
             MemberOfOrganizationId = orgId,
             FullName = "Yo lama1",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         var userPersistence2 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test2@test.com",
             OwnedOrganizationId = null,
             MemberOfOrganizationId = orgId,
             FullName = "Yo lama2",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         
         await PopulateDatabase(new[] {userPersistence1});
         await PopulateDatabase(new[] {userPersistence2});
         
         // Act
         var httpResponseMessage = await TestClient.DeleteAsync(new Uri($"api/v1/Organization/Remove/user/{userPersistence1.Id.ToString()}", UriKind.Relative));
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

         var user = await GetAll<UserPersistenceModel>();
         user.Length.Should().Be(2);
         var user2 = user.FirstOrDefault(x => x.Id == userPersistence2.Id);
         user2?.Id.Should().Be(userPersistence2.Id);
         user2?.Email.Should().Be(userPersistence2.Email);
     }
     
      [Fact]
     public async Task RemoveUserFromOrganization_OwnerRemoveForbidden() // Change auth attribute to pass
     {
         // Arrange 
         var orgId = IdGenerator.GenerateId();
         await Authenticate();
         var userPersistence1 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test1@test.com",
             OwnedOrganizationId = "aa",
             MemberOfOrganizationId = orgId,
             FullName = "Yo lama1",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         var userPersistence2 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test2@test.com",
             OwnedOrganizationId = "",
             MemberOfOrganizationId = orgId,
             FullName = "Yo lama2",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         
         await PopulateDatabase(new[] {userPersistence1});
         await PopulateDatabase(new[] {userPersistence2});
         
         // Act
         var httpResponseMessage = await TestClient.DeleteAsync(new Uri($"api/v1/Organization/Remove/user/{userPersistence1.Id.ToString()}", UriKind.Relative));
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Forbidden);

         var user = await GetAll<UserPersistenceModel>();
         user.Length.Should().Be(3); // Because one is for auth service
     }
}