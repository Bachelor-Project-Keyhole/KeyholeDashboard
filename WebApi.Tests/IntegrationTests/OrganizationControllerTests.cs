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
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
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
            hasAccepted = false,
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
            hasAccepted = false,
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
            hasAccepted = false,
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
            hasAccepted = true,
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
    public async Task UpdateOrganization_Successful() 
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var request = new UpdateOrganizationRequest
        {
            OrganizationName = "Changed Name",
            OrganzationId = organization.Id.ToString()
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
            OrganzationId = IdGenerator.GenerateId()
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
            hasAccepted = false,
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
            AccessLevels = new List<UserAccessLevel> { UserAccessLevel.Viewer, UserAccessLevel.Editor },
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
            AdminUserId = userPersistence2.Id.ToString(),
            SetAccessLevel = UserAccessLevel.Editor
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
    public async Task ChangeAccessLevelOfUser_AdminUserNotFound() 
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
            AdminUserId = IdGenerator.GenerateId(),
            SetAccessLevel = UserAccessLevel.Editor
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
        demotedUser?.AccessLevels.Should().Contain(UserAccessLevel.Admin);
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
            AdminUserId = userPersistence2.Id.ToString(),
            SetAccessLevel = UserAccessLevel.Editor
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
    public async Task ChangeAccessLevelOfUser_CannotChangeOwnerForbiddenException() 
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
            OwnedOrganizationId = "Admin",
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
            AdminUserId = userPersistence2.Id.ToString(),
            SetAccessLevel = UserAccessLevel.Editor
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Organization/access", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var response =
            JsonConvert.DeserializeObject<UserChangeAccessResponse>(
                await httpResponseMessage.Content.ReadAsStringAsync());
        var demotedUser =
            (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Id.ToString() == response?.UserId);
        demotedUser?.AccessLevels.Should().NotContain(UserAccessLevel.Admin);
    }

    [Fact]
    public async Task ChangeAccessLevelOfUser_DifferentOrganizationIdException() 
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
            MemberOfOrganizationId = IdGenerator.GenerateId(),
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
            AdminUserId = userPersistence2.Id.ToString(),
            SetAccessLevel = UserAccessLevel.Editor
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"api/v1/Organization/access", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var response =
            JsonConvert.DeserializeObject<UserChangeAccessResponse>(
                await httpResponseMessage.Content.ReadAsStringAsync());
        var demotedUser =
            (await GetAll<UserPersistenceModel>()).FirstOrDefault(x => x.Id.ToString() == response?.UserId);
        demotedUser?.AccessLevels.Should().NotContain(UserAccessLevel.Admin);
    }
}