using System.Net;
using System.Text;
using Application.JWT.Helper;
using Application.Organization.Model;
using Application.User.Model;
using Domain;
using Domain.User;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Organization;
using Repository.User.UserPersistence;

namespace WebApi.Tests.IntegrationTests.OrganizationController;

public class OrganizationControllerTests : IntegrationTest
{

    [Fact]
    public async Task PostOrganizationAndUser_Successful()
    {
        // Arrange
        // var userPersistence = new UserPersistenceModel
        // {
        //     Id = ObjectId.Parse(IdGenerator.GenerateId()),
        //     Email = "test@test.com",
        //     OwnedOrganizationId = "",
        //     MemberOfOrganizationId = "",
        //     FullName = "Yo lama",
        //     PasswordHash = PasswordHelper.GetHashedPassword("orange1234"), // Has to be atleast 8 chars
        //     AccessLevels = new List<UserAccessLevel>
        //         {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
        //     RefreshTokens = new List<PersistenceRefreshToken>(),
        //     ModifiedDate = DateTime.UtcNow,
        //     RegistrationDate = DateTime.UtcNow
        // };
        //
        //
        //
        // var organizationPersistence = new OrganizationPersistenceModel
        // {
        //     Id = ObjectId.Parse(IdGenerator.GenerateId()),
        //     OrganizationOwnerId = "",
        //     OrganizationName = "OrgName",
        //     Members = new List<PersistenceOrganizationMembers>(),
        //     CreationDate = DateTime.UtcNow,
        //     ModificationDate = DateTime.UtcNow
        // };

        // await PopulateDatabase(new OrganizationPersistenceModel[] {organizationPersistence});
        // await PopulateDatabase(new UserPersistenceModel[] {userPersistence});

        var request = new CreateAdminAndOrganizationRequest
        {
            Email = "test@testRequest.com",
            FullName = "Yo lama",
            OrganizationName = "OrgName11",
            Password = "orange1234"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage = await TestClient.PostAsync(new Uri($"/api/v1/Organization/register", UriKind.Relative), stringContent);
        
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var v = JsonConvert.DeserializeObject<AdminAndOrganizationCreateResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
        v.
        var allOrganization = await GetAll<OrganizationPersistenceModel>();
        var allUser = await GetAll<UserPersistenceModel>();

        var organization = allOrganization.Single();
        organization.OrganizationName.Should().Be(request.OrganizationName);
        
        
        var user = allUser.Single();
        user.Email.Should().Be(request.Email);
        user.FullName.Should().Be(request.FullName);
        user.PasswordHash.Should().Be(PasswordHelper.GetHashedPassword("orange1234"));
        
        organization



    }
}