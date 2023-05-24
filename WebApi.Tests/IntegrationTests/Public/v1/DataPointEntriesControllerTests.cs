using System.Net;
using System.Text;
using Contracts;
using Domain;
using FluentAssertions;
using Newtonsoft.Json;
using Repository.Datapoint;
using WebApi.Controllers.Public.v1;

namespace WebApi.Tests.IntegrationTests.Public.v1;

public class DataPointEntriesControllerTests : IntegrationTest
{
    // [Fact]
    // public async Task PostDataPointEntry_PersistsEntryInDatabase_AndCreatesNewDataPoint()
    // {
    //     //Arrange
    //     await Authenticate();
    //
    //     var organization = await SetupOrganization();
    //
    //     var dataPointEntryDto =
    //         new PushDataPointEntryDto(organization.Id.ToString(), IdGenerator.GenerateId(), 500, DateTime.Now);
    //
    //     var stringContent =
    //         new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
    //
    //     //Act
    //     var httpResponseMessage =
    //         await TestClient.PostAsync(new Uri("/api/public/v1/DataPointEntries", UriKind.Relative),
    //             stringContent);
    //
    //     //Assert
    //     httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    //     var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
    //     dataPointEntryEntities.Should().NotBeNull();
    //     var dataPointEntryEntity = dataPointEntryEntities.Single();
    //     dataPointEntryEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
    //     dataPointEntryEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
    //     dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
    //     dataPointEntryEntity.Time.Should().BeCloseTo(dataPointEntryEntity.Time, TimeSpan.FromSeconds(1));
    //
    //     var dataPointEntities = await GetAll<DataPointEntity>();
    //     var dataPointEntity = dataPointEntities.Single();
    //     dataPointEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
    //     dataPointEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
    //     dataPointEntity.DisplayName.Should().Be(dataPointEntryDto.DataPointKey);
    //     dataPointEntity.ComparisonIsAbsolute.Should().BeFalse();
    //     dataPointEntity.DirectionIsUp.Should().BeTrue();
    //     dataPointEntity.LatestValue.Should().Be(dataPointEntryDto.Value);
    // }
    //
    // [Fact]
    // public async Task PostDataPointEntry_PersistsEntryIntoDatabase_DoesNotCreateDuplicateDataPoint()
    // {
    //     //Arrange
    //     var organization = await SetupOrganization();
    //
    //     var key = "TestKey";
    //     var dataPointEntity = new DataPointEntity(organization.Id.ToString(), key, key);
    //     await PopulateDatabase(new[] { dataPointEntity });
    //
    //     var dataPointEntryDto =
    //         new PushDataPointEntryDto(organization.Id.ToString(), key, 500, DateTime.Now);
    //
    //     var stringContent =
    //         new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
    //
    //     //Act
    //     var httpResponseMessage =
    //         await TestClient.PostAsync(new Uri("/api/public/v1/DataPointEntries", UriKind.Relative),
    //             stringContent);
    //
    //     //Assert
    //     httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    //     var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
    //     var dataPointEntryEntity = dataPointEntryEntities.Single();
    //     dataPointEntryEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
    //     dataPointEntryEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
    //     dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
    //     dataPointEntryEntity.Time.Should().BeCloseTo(dataPointEntryEntity.Time, TimeSpan.FromSeconds(1));
    //
    //     var dataPointEntities = await GetAll<DataPointEntity>();
    //     dataPointEntities.Should().HaveCount(1);
    //     dataPointEntities.Single().LatestValue.Should().Be(dataPointEntryDto.Value);
    // }
    //
    // [Fact]
    // public async Task PostDataPointEntry_ReturnsNotFound_WhenOrganizationIdDoesNotExists()
    // {
    //     //Arrange
    //     await Authenticate();
    //     var nonExistingOrganizationId = IdGenerator.GenerateId();
    //
    //     var dataPointEntryDto =
    //         new PushDataPointEntryDto(nonExistingOrganizationId, IdGenerator.GenerateId(), 500, DateTime.Now);
    //
    //     var stringContent =
    //         new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
    //
    //     //Act
    //     var httpResponseMessage =
    //         await TestClient.PostAsync(new Uri("/api/public/v1/DataPointEntries", UriKind.Relative),
    //             stringContent);
    //
    //     //Assert
    //     httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    //     var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
    //     dataPointEntryEntities.Should().BeEmpty();
    // }
    //
    // [Fact]
    // public async Task PostNewDataPointEntries_PersistsEntriesInDatabase()
    // {
    //     //Arrange
    //     var organization = await SetupOrganization();
    //
    //     var dataPointEntryDto =
    //         new PushDataPointEntryDto(organization.Id.ToString(), IdGenerator.GenerateId(), 500, DateTime.Now);
    //
    //     var stringContent =
    //         new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
    //
    //     //Act
    //     var httpResponseMessage =
    //         await TestClient.PostAsync(new Uri("/api/public/v1/DataPointEntries", UriKind.Relative),
    //             stringContent);
    //
    //     //Assert
    //     httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    //     var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
    //     dataPointEntryEntities.Should().NotBeNull();
    //     var dataPointEntryEntity = dataPointEntryEntities.Single();
    //     dataPointEntryEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
    //     dataPointEntryEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
    //     dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
    //     dataPointEntryEntity.Time.Should().BeCloseTo(dataPointEntryEntity.Time, TimeSpan.FromSeconds(1));
    //
    //     var dataPointEntities = await GetAll<DataPointEntity>();
    //     var dataPointEntity = dataPointEntities.Single();
    //     dataPointEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
    //     dataPointEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
    //     dataPointEntity.DisplayName.Should().Be(dataPointEntryDto.DataPointKey);
    //     dataPointEntity.ComparisonIsAbsolute.Should().BeFalse();
    //     dataPointEntity.DirectionIsUp.Should().BeTrue();
    //     dataPointEntity.LatestValue.Should().Be(dataPointEntryDto.Value);
    // }
}