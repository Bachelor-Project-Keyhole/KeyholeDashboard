using System.Net;
using System.Text;
using Application.JWT.Helper;
using Application.User.Model;
using Contracts.v1.Organization;
using Domain;
using Domain.User;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Organization;
using Repository.OrganizationUserInvite;
using Repository.User.UserPersistence;

namespace WebApi.Tests.IntegrationTests;

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
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Organization/register", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<AdminAndOrganizationCreateResponse>(await httpResponseMessage.Content
                .ReadAsStringAsync());
        var allOrganization = await GetAll<OrganizationPersistenceModel>();
        var allUser = await GetAll<UserPersistenceModel>();

        var organization = allOrganization.Single();
        organization.OrganizationName.Should().Be(request.OrganizationName);
        if (DateTime.UtcNow.Hour != organization.CreationDate.Hour)
        {
            response?.OrganizationId.Should().Be(organization.Id.ToString());
            organization.CreationDate.Should().NotBe(response?.OrganizationCreationTime);
        }

        var user = allUser.Single();
        user.Id.ToString().Should().Be(response?.UserId);
        if (DateTime.UtcNow.Hour != user.RegistrationDate.Hour)
            response?.OrganizationCreationTime.Should().NotBe(user.RegistrationDate);
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
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Organization/register", UriKind.Relative), stringContent);

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
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };
        await PopulateDatabase(new[] { userPersistence });


        var request = new CreateAdminAndOrganizationRequest
        {
            Email = "test@test.com",
            FullName = "Yo lama",
            OrganizationName = "OrgName11",
            Password = "orange1223311"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Organization/register", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var allOrganization = await GetAll<OrganizationPersistenceModel>();
        var allUser = await GetAll<UserPersistenceModel>();
        allOrganization.Should().BeNullOrEmpty();
        allUser.Should().HaveCount(1);
    }

    [Fact]
    public async Task InviteUser_Successful() 
    {
        // Arrange
        await Authenticate();
        var userPersistence = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "test1@test1.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "",
            FullName = "Yo lama1",
            PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };


        var organizationPersistence = new OrganizationPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationOwnerId = "",
            OrganizationName = "OrgNam1e",
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { organizationPersistence });
        await PopulateDatabase(new[] { userPersistence });

        var request = new OrganizationUserInviteRequest
        {
            OrganizationId = organizationPersistence.Id.ToString(),
            AccessLevel = "Admin",
            ReceiverEmailAddress = "dziugis10@gmail.com",
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/organization/invite/email", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task InviteUser_EnumParseException() 
    {
        // Arrange
        await Authenticate();
        var userPersistence = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "test1@test1.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "",
            FullName = "Yo lama1",
            PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };


        var organizationPersistence = new OrganizationPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationOwnerId = "",
            OrganizationName = "OrgNam1e",
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { organizationPersistence });
        await PopulateDatabase(new[] { userPersistence });

        var request = new OrganizationUserInviteRequest
        {
            OrganizationId = organizationPersistence.Id.ToString(),
            AccessLevel = "a",
            ReceiverEmailAddress = "dziugis10@gmail.com",
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/organization/invite/email", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task InviteUser_RemoveOutDatedInvitations() 
    {
        // Arrange
        await Authenticate();
        var userPersistence = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "test1@test1.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "",
            FullName = "Yo lama1",
            PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };


        var organizationPersistence = new OrganizationPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationOwnerId = "",
            OrganizationName = "OrgNam1e",
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { organizationPersistence });
        await PopulateDatabase(new[] { userPersistence });

        var invitation1 = new OrganizationUserInvitePersistence
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Token = "tCcihCry",
            AccessLevels = new List<UserAccessLevel> {UserAccessLevel.Viewer, UserAccessLevel.Editor},
            HasAccepted = false,
            OrganizationId = organizationPersistence.Id.ToString(),
            ReceiverEmail = "DoesNotMatter@DoesNotMatter.Ok",
            TokenExpirationTime = DateTime.UtcNow.AddDays(-5),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(-2)
        };
        
        var invitation2 = new OrganizationUserInvitePersistence
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Token = "tCcihary",
            AccessLevels = new List<UserAccessLevel> {UserAccessLevel.Viewer, UserAccessLevel.Editor},
            HasAccepted = false,
            OrganizationId = organizationPersistence.Id.ToString(),
            ReceiverEmail = "DoesNaotMatter@DoesNotMaatter.Ok",
            TokenExpirationTime = DateTime.UtcNow.AddDays(-5),
            RemoveFromDbDate = DateTime.UtcNow.AddHours(-1)
        };
 
        await PopulateDatabase(new[] { invitation1, invitation2 });


        var request = new OrganizationUserInviteRequest
        {
            OrganizationId = organizationPersistence.Id.ToString(),
            AccessLevel = "Admin",
            ReceiverEmailAddress = "dziugis10@gmail.com",
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/organization/invite/email", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var invitations = await GetAll<OrganizationUserInvitePersistence>();
        invitations.Length.Should().Be(1);
        var invitation = invitations.Single();
        invitation.Id.Should().NotBe(invitation1.Id);
        invitation.Id.Should().NotBe(invitation2.Id);
    }
    
    [Fact]
    public async Task CompleteRegistrationByInvitation_Successful() 
    {
        // Arrange
        await Authenticate();
        var invitationOfUser = new OrganizationUserInvitePersistence
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationId = IdGenerator.GenerateId(),
            Token = "5a6f8a",
            ReceiverEmail = "test@test.com",
            HasAccepted = false,
            TokenExpirationTime = DateTime.UtcNow.AddDays(2),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(5),
            AccessLevels = new List<UserAccessLevel> { UserAccessLevel.Viewer, UserAccessLevel.Editor },
        };

        await PopulateDatabase(new[] { invitationOfUser });

        var request = new UserRegistrationRequest
        {
            FullName = "fName",
            Password = "orange1234"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage = await TestClient.PostAsync(
            new Uri($"/api/v1/Organization/register/{invitationOfUser.Token}", UriKind.Relative), stringContent);
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<UserRegistrationResponse>(
                await httpResponseMessage.Content.ReadAsStringAsync());
        response?.Email.Should().Be(invitationOfUser.ReceiverEmail);
        response?.FullName.Should().Be(request.FullName);
        response?.OrganizationId.Should().Be(invitationOfUser.OrganizationId);
        var allUser = await GetAll<UserPersistenceModel>();
        var user = allUser.Single(x => x.FullName == request.FullName);
        if (DateTime.UtcNow.Hour != user.RegistrationDate.Hour)
            user.RegistrationDate.ToString().Should().NotBe(response?.RegistrationDate);
    }

    [Fact]
    public async Task CompleteRegistrationByInvitation_InvalidToken() 
    {
        // Arrange
        await Authenticate();
        var invitationOfUser = new OrganizationUserInvitePersistence
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationId = IdGenerator.GenerateId(),
            Token = "5a6f8a",
            ReceiverEmail = "test@test.com",
            HasAccepted = false,
            TokenExpirationTime = DateTime.UtcNow.AddDays(2),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(5),
            AccessLevels = new List<UserAccessLevel> { UserAccessLevel.Viewer, UserAccessLevel.Editor },
        };

        await PopulateDatabase(new[] { invitationOfUser });

        var request = new UserRegistrationRequest
        {
            FullName = "fName",
            Password = "orange1234"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Organization/register/aaaaaa", UriKind.Relative),
                stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteRegistrationByInvitation_ExpiredToken() 
    {
        // Arrange
        await Authenticate();
        var invitationOfUser = new OrganizationUserInvitePersistence
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationId = IdGenerator.GenerateId(),
            Token = "5a6f8a",
            ReceiverEmail = "test@test.com",
            HasAccepted = false,
            TokenExpirationTime = DateTime.UtcNow.AddDays(-2),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(1),
            AccessLevels = new List<UserAccessLevel> { UserAccessLevel.Viewer, UserAccessLevel.Editor },
        };

        await PopulateDatabase(new[] { invitationOfUser });

        var request = new UserRegistrationRequest
        {
            FullName = "fName",
            Password = "orange1234"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage = await TestClient.PostAsync(
            new Uri($"/api/v1/Organization/register/{invitationOfUser.Token}", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteRegistrationByInvitation_TokenAlreadyUsed() 
    {
        // Arrange
        await Authenticate();
        var invitationOfUser = new OrganizationUserInvitePersistence
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationId = IdGenerator.GenerateId(),
            Token = "5a6f8a",
            ReceiverEmail = "test@test.com",
            HasAccepted = true,
            TokenExpirationTime = DateTime.UtcNow.AddDays(2),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(1),
            AccessLevels = new List<UserAccessLevel> { UserAccessLevel.Viewer, UserAccessLevel.Editor },
        };

        await PopulateDatabase(new[] { invitationOfUser });

        var request = new UserRegistrationRequest
        {
            FullName = "fName",
            Password = "orange1234"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage = await TestClient.PostAsync(
            new Uri($"/api/v1/Organization/register/{invitationOfUser.Token}", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrganizationById_Successful()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Organization/{organization.Id.ToString()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<OrganizationDetailedResponse>(await httpResponseMessage.Content
                .ReadAsStringAsync());

        response?.OrganizationId.Should().Be(organization.Id.ToString());
        response?.OrganizationName.Should().Be(organization.OrganizationName);
        response?.ApiKey.Should().Be(organization.ApiKey);
        response?.OrganizationOwnerId.Should().Be(organization.OrganizationOwnerId);
    }

    [Fact]
    public async Task GetOrganizationById_NotFound()
    {
        // Arrange
        await Authenticate();

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Organization/{IdGenerator.GenerateId()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllUsersOfOrganization() 
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
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
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
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistence1 });
        await PopulateDatabase(new[] { userPersistence2 });

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"api/v1/Organization/users/{orgId}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var response =
            JsonConvert.DeserializeObject<AllUsersOfOrganizationResponse>(await httpResponseMessage.Content
                .ReadAsStringAsync());
        response?.Users?.Count.Should().Be(2);
        response?.OrganizationId.Should().Be(orgId);

        var user1 = response?.Users?.Single(x => x.Id == userPersistence1.Id.ToString() && x.Email == userPersistence1.Email);
        var user2 = response?.Users?.Single(x => x.Id == userPersistence2.Id.ToString() && x.Email == userPersistence2.Email);
        response?.Users.Should().Contain(user1!);
        response?.Users.Should().Contain(user2!);
    }


    [Fact]
    public async Task GetInvitationById_Successful()
    {
        // Assert
        await Authenticate();
        var organization = await SetupOrganization();
        var invitation = await InsertInvitation(organization.Id.ToString());
        
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Organization/invitationId/{invitation.Id.ToString()}/organizationId/{organization.Id.ToString()}",
            UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<PendingUserInvitationResponse>(await httpResponseMessage.Content.ReadAsStringAsync());

        response?.OrganizationId.Should().Be(organization.Id.ToString());
        response?.Token.Should().Be(invitation.Token);
        response?.ReceiverEmail.Should().Be(invitation.ReceiverEmail);
        response?.HasAccepted.Should().Be(invitation.HasAccepted);
    }
    
    [Fact]
    public async Task GetInvitationById_OrganizationNotFound()
    {
        // Assert
        await Authenticate();
        var randomId = IdGenerator.GenerateId();
        var invitation = await InsertInvitation(randomId);
        
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Organization/invitationId/{invitation.Id.ToString()}/organizationId/{randomId}",
            UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var invites = await GetAll<OrganizationUserInvitePersistence>();
        invites.Length.Should().Be(1);
    }
    
    [Fact]
    public async Task GetInvitationById_InvitationNotFound()
    {
        // Assert
        await Authenticate();
        var organization = await SetupOrganization();
        var randomId = IdGenerator.GenerateId();

        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Organization/invitationId/{randomId}/organizationId/{organization.Id.ToString()}",
            UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var invites = await GetAll<OrganizationUserInvitePersistence>();
        invites.Length.Should().Be(0);
    }
    
    [Fact]
    public async Task GetInvitationByEmail_Successful()
    {
        // Assert
        await Authenticate();
        var organization = await SetupOrganization();
        var invitation = await InsertInvitation(organization.Id.ToString());
        
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Organization/invitation/email/{invitation.ReceiverEmail}/organizationId/{organization.Id.ToString()}",
            UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<PendingUserInvitationResponse>(await httpResponseMessage.Content.ReadAsStringAsync());

        response?.OrganizationId.Should().Be(organization.Id.ToString());
        response?.Token.Should().Be(invitation.Token);
        response?.ReceiverEmail.Should().Be(invitation.ReceiverEmail);
        response?.HasAccepted.Should().Be(invitation.HasAccepted);
    }
    
    [Fact]
    public async Task GetInvitationByEmail_OrganizationNotFound()
    {
        // Assert
        await Authenticate();
        var randomId = IdGenerator.GenerateId();
        var invitation = await InsertInvitation(randomId);
        
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Organization/invitation/email/{invitation.ReceiverEmail}/organizationId/{randomId}",
            UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var invites = await GetAll<OrganizationUserInvitePersistence>();
        invites.Length.Should().Be(1);
    }

    [Fact]
    public async Task GetInvitationByEmail_InvitationNotFound()
    {
        // Assert
        await Authenticate();
        var organization = await SetupOrganization();
        var randomId = IdGenerator.GenerateId();

        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Organization/invitation/email/{randomId}/organizationId/{organization.Id.ToString()}",
            UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var invites = await GetAll<OrganizationUserInvitePersistence>();
        invites.Length.Should().Be(0);
    }
    
    [Fact]
    public async Task GetInvitationByOrganizationId_Successful()
    {
        // Assert
        await Authenticate();
        var organization = await SetupOrganization();
        var randomId = IdGenerator.GenerateId();

        var invitation1 = await InsertInvitation(organization.Id.ToString());
        var invitation2 = new OrganizationUserInvitePersistence
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Token = "tCcaaCry",
            AccessLevels = new List<UserAccessLevel> {UserAccessLevel.Viewer, UserAccessLevel.Editor},
            HasAccepted = false,
            OrganizationId = organization.Id.ToString(),
            ReceiverEmail = "DoaesNotMatter@DoesNaotMatter.Ok",
            TokenExpirationTime = DateTime.UtcNow.AddDays(2),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(3)
        };
        
        await PopulateDatabase(new[] { invitation2 });

        await InsertInvitation(randomId);
        
        
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Organization/invitation/organization/{organization.Id.ToString()}",
            UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<List<PendingUserInvitationResponse>>(await httpResponseMessage.Content.ReadAsStringAsync());

        var invitations = await GetAll<OrganizationUserInvitePersistence>();
        invitations.Length.Should().Be(3);
        response?.Count.Should().Be(2);

        var response1 = response?.Single(x => x.Token == invitation1.Token);
        response1?.OrganizationId.Should().Be(organization.Id.ToString());
        response1?.ReceiverEmail.Should().Be(invitation1.ReceiverEmail);
        response1?.HasAccepted.Should().Be(invitation1.HasAccepted);
        
        var response2 = response?.Single(x => x.Token == invitation2.Token); 
        response1?.OrganizationId.Should().Be(organization.Id.ToString());
        response2?.ReceiverEmail.Should().Be(invitation2.ReceiverEmail);
        response2?.HasAccepted.Should().Be(invitation2.HasAccepted);
    }
    
    
    [Fact]
    public async Task UpdateOrganization_Successful() 
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var request = new UpdateOrganizationRequest
        {
            OrganizationName = "Changed Name",
            OrganizationId = organization.Id.ToString()
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PutAsync(new Uri($"/api/v1/Organization/update", UriKind.Relative), stringContent);

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<OrganizationResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
        response?.OrganizationName.Should().Be(request.OrganizationName);
        var allOrg = await GetAll<OrganizationPersistenceModel>();
        var org = allOrg.Single(x => x.OrganizationName == request.OrganizationName);
        if (DateTime.UtcNow.Hour != org.CreationDate.Hour)
        {
            org.CreationDate.Should().NotBe(response?.CreationDate);
            org.ModificationDate.Should().NotBe(response?.ModificationDate);
        }
    }

    [Fact]
    public async Task UpdateOrganization_NotFound() 
    {
        // Arrange
        await Authenticate();
        await SetupOrganization();

        var request = new UpdateOrganizationRequest
        {
            OrganizationName = "Changed Name",
            OrganizationId = IdGenerator.GenerateId()
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PutAsync(new Uri($"/api/v1/Organization", UriKind.Relative), stringContent);

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveUserFromOrganization_Successful() 
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
                { UserAccessLevel.Viewer, UserAccessLevel.Editor },
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
                { UserAccessLevel.Viewer, UserAccessLevel.Editor },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistence1 });
        await PopulateDatabase(new[] { userPersistence2 });

        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"api/v1/Organization/Remove/user/{userPersistence1.Id.ToString()}",
                UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await GetAll<UserPersistenceModel>();
        user.Length.Should().Be(2);
        var user2 = user.FirstOrDefault(x => x.Id == userPersistence2.Id);
        user2?.Id.Should().Be(userPersistence2.Id);
        user2?.Email.Should().Be(userPersistence2.Email);
    }

    [Fact]
    public async Task RemoveUserFromOrganization_OwnerRemoveForbidden() 
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
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
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
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistence1 });
        await PopulateDatabase(new[] { userPersistence2 });

        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"api/v1/Organization/Remove/user/{userPersistence1.Id.ToString()}",
                UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var user = await GetAll<UserPersistenceModel>();
        user.Length.Should().Be(3); // Because one is for auth service
    }
    
    [Fact]
    public async Task RemoveUserInvitationPastTtl()
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
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
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

        var outdatedInvitation = new OrganizationUserInvitePersistence
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            HasAccepted = false,
            Token = "randomString",
            OrganizationId = organizationPersistence.Id.ToString(),
            AccessLevels = new List<UserAccessLevel> { UserAccessLevel.Viewer },
            ReceiverEmail = "random@random.com",
            TokenExpirationTime = DateTime.UtcNow.AddDays(-10),
            RemoveFromDbDate = DateTime.UtcNow.AddDays(-2),
        };

        await PopulateDatabase(new[] { organizationPersistence });
        await PopulateDatabase(new[] { userPersistence });
        await PopulateDatabase(new[] { outdatedInvitation });

        var request = new OrganizationUserInviteRequest
        {
            OrganizationId = organizationPersistence.Id.ToString(),
            AccessLevel = "Viewer",
            ReceiverEmailAddress = "dziugis10@gmail.com",
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Organization/invite/email", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var invitations = await GetAll<OrganizationUserInvitePersistence>();
        invitations.Length.Should().Be(1);
    }

    [Fact]
    public async Task RemoveInvitationById_Successful()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();
        var invitation1 = await InsertInvitation(organization.Id.ToString());
        var invitation2 = await InsertInvitation(organization.Id.ToString());
        
        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri(
                $"/api/v1/Organization/invitation/id/{invitation1.Id.ToString()}/organizationId/{organization.Id.ToString()}", UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var invitations = await GetAll<OrganizationUserInvitePersistence>();
        invitations.Length.Should().Be(1);
        var invitation = invitations.Single();
        invitation.Id.Should().Be(invitation2.Id);
    }
    
    [Fact]
    public async Task RemoveInvitationById_OrganizationNotFound()
    {
        // Arrange
        await Authenticate();
        var randomId = IdGenerator.GenerateId();
        var invitation1 = await InsertInvitation(randomId);
        var invitation2 = await InsertInvitation(randomId);
        
        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri(
                $"/api/v1/Organization/invitation/id/{invitation1.Id.ToString()}/organizationId/{randomId}", UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var invitations = await GetAll<OrganizationUserInvitePersistence>();
        invitations.Length.Should().Be(2);
    }
    
    [Fact]
    public async Task RemoveInvitationById_InvitationNotFound()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();
        var randomId = IdGenerator.GenerateId();
        var invitation2 = await InsertInvitation(organization.Id.ToString());
        
        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri(
                $"/api/v1/Organization/invitation/id/{randomId}/organizationId/{organization.Id.ToString()}", UriKind.Relative));
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var invitations = await GetAll<OrganizationUserInvitePersistence>();
        invitations.Length.Should().Be(1);
    }

    [Fact]
    public async Task ChangeAccessLevelOfUser_Successful() 
    {
        // Arrange
        var orgId = IdGenerator.GenerateId();
        await Authenticate();
        var userPersistence2 = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "test1@test.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = orgId,
            FullName = "Yo lama1a",
            PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };
        var userPersistence3 = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "test2@test.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = orgId,
            FullName = "Yo lama2",
            PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistence2 });
        await PopulateDatabase(new[] { userPersistence3 });

        var request = new ChangeUserAccessRequest
        {
            UserId = userPersistence3.Id.ToString(),
            SetAccessLevel = "Editor"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Organization/access", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var response =
            JsonConvert.DeserializeObject<UserChangeAccessResponse>(
                await httpResponseMessage.Content.ReadAsStringAsync());
        var demotedUser =
            (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Id.ToString() == response?.UserId);
        demotedUser?.AccessLevels.Should().NotContain(UserAccessLevel.Admin);
    }

    [Fact]
    public async Task ChangeAccessLevelOfUser_UserNotFound() 
    {
        // Arrange
        var orgId = IdGenerator.GenerateId();
        await Authenticate();
        var userPersistence2 = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "test1@test.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = orgId,
            FullName = "Yo lama1a",
            PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };
        var userPersistence3 = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "test2@test.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = orgId,
            FullName = "Yo lama2",
            PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistence2 });
        await PopulateDatabase(new[] { userPersistence3 });

        var request = new ChangeUserAccessRequest
        {
            UserId = IdGenerator.GenerateId(),
            SetAccessLevel = "Editor"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Organization/access", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var response =
            JsonConvert.DeserializeObject<UserChangeAccessResponse>(
                await httpResponseMessage.Content.ReadAsStringAsync());
        var demotedUser =
            (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Id.ToString() == response?.UserId);
    }

    
    [Fact]
     public async Task ChangeAccessLevelOfUser_AccessLevelParseException() // Change auth attribute to pass
     {
         // Arrange
         await Authenticate();
         var userPersistence2 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test1@test.com",
             OwnedOrganizationId = "",
             MemberOfOrganizationId = IdGenerator.GenerateId(),
             FullName = "Yo lama1a",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         var userPersistence3 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test2@test.com",
             OwnedOrganizationId = "",
             MemberOfOrganizationId = IdGenerator.GenerateId(),
             FullName = "Yo lama2",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         
         await PopulateDatabase(new [] {userPersistence2});
         await PopulateDatabase(new[] {userPersistence3});

         var request = new ChangeUserAccessRequest
         {
             UserId = userPersistence3.Id.ToString(),
             SetAccessLevel = "E"
         };
         
         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

         // Act
         var httpResponseMessage = await TestClient.PostAsync(new Uri($"api/v1/Organization/access", UriKind.Relative), stringContent);
         
         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Forbidden);
     }

     [Theory]
     [InlineData("Viewer")]
     [InlineData("Editor")]
     [InlineData("Admin")]
     public async Task
         ChangeAccessLevelOfUser_AccessLevels(params string[] accessLevels) // Change auth attribute to pass
     {
         // Arrange
         var orgId = IdGenerator.GenerateId();
         await Authenticate();
         var userPersistence2 = new UserPersistenceModel
         {
             Id = ObjectId.Parse(IdGenerator.GenerateId()),
             Email = "test1@test.com",
             OwnedOrganizationId = "",
             MemberOfOrganizationId = orgId,
             FullName = "Yo lama1a",
             PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be at least 8 chars
             AccessLevels = new List<UserAccessLevel>
                 {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
             RefreshTokens = new List<PersistenceRefreshToken>(),
             ModifiedDate = DateTime.UtcNow,
             RegistrationDate = DateTime.UtcNow
         };
         var userPersistence3 = new UserPersistenceModel
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

         await PopulateDatabase(new[] {userPersistence2});
         await PopulateDatabase(new[] {userPersistence3});

         var request = new ChangeUserAccessRequest
         {
             UserId = userPersistence3.Id.ToString(),
             SetAccessLevel = accessLevels.Single()
         };

         var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

         // Act
         var httpResponseMessage =
             await TestClient.PostAsync(new Uri($"api/v1/Organization/access", UriKind.Relative), stringContent);

         // Assert
         httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

         var response =
             JsonConvert.DeserializeObject<UserChangeAccessResponse>(
                 await httpResponseMessage.Content.ReadAsStringAsync());
         var demotedUser =
             (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Id.ToString() == response?.UserId);
         switch (accessLevels.Single())
         {
             case "Viewer":
                 demotedUser?.AccessLevels.Should().NotContain(UserAccessLevel.Editor);
                 demotedUser?.AccessLevels.Should().NotContain(UserAccessLevel.Admin);
                 demotedUser?.AccessLevels.Should().Contain(UserAccessLevel.Viewer);
                 break;
             case "Editor":
                 demotedUser?.AccessLevels.Should().Contain(UserAccessLevel.Viewer);
                 demotedUser?.AccessLevels.Should().Contain(UserAccessLevel.Editor);
                 demotedUser?.AccessLevels.Should().NotContain(UserAccessLevel.Admin);
                 break;
             case "Admin":
                 demotedUser?.AccessLevels.Should().Contain(UserAccessLevel.Viewer);
                 demotedUser?.AccessLevels.Should().Contain(UserAccessLevel.Editor);
                 demotedUser?.AccessLevels.Should().Contain(UserAccessLevel.Admin);
                 break;
         }
     }
}