using System.Net;
using System.Text;
using Domain;
using FluentAssertions;
using Newtonsoft.Json;
using Repository.Datapoint;
using WebApi.Controllers.Public.v1;

namespace WebApi.Tests.IntegrationTests.Public.v1;

public class DataPointEntriesControllerTests : IntegrationTest
{
    [Fact]
    public async Task PostDataPointEntry_PersistsEntryInDatabase_AndCreatesNewDataPoint()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();
    
        var dataPointEntryDto =
            new PushDataPointEntryDto(IdGenerator.GenerateId(), 500);
    
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
    
        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"/api/public/v1/DataPointEntries/{organization.ApiKey}/single", UriKind.Relative),
                stringContent);
    
        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().NotBeNull();
        var dataPointEntryEntity = dataPointEntryEntities.Single();
        dataPointEntryEntity.OrganizationId.Should().Be(organization.Id.ToString());
        dataPointEntryEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
        dataPointEntryEntity.Time.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    
        var dataPointEntities = await GetAll<DataPointEntity>();
        var dataPointEntity = dataPointEntities.Single();
        dataPointEntity.OrganizationId.Should().Be(organization.Id.ToString());
        dataPointEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntity.DisplayName.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntity.ComparisonIsAbsolute.Should().BeFalse();
        dataPointEntity.DirectionIsUp.Should().BeTrue();
        dataPointEntity.LatestValue.Should().Be(dataPointEntryDto.Value);
    }
    
    [Fact]
    public async Task PostDataPointEntry_PersistsEntryIntoDatabase_DoesNotCreateDuplicateDataPoint()
    {
        //Arrange
        var organization = await SetupOrganization();
    
        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), key, key);
        await PopulateDatabase(new[] { dataPointEntity });
    
        var dataPointEntryDto =
            new PushDataPointEntryDto(key, 500);
    
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
    
        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"/api/public/v1/DataPointEntries/{organization.ApiKey}/single", UriKind.Relative),
                stringContent);
    
        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        var dataPointEntryEntity = dataPointEntryEntities.Single();
        dataPointEntryEntity.OrganizationId.Should().Be(organization.Id.ToString());
        dataPointEntryEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
        dataPointEntryEntity.Time.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    
        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(1);
        dataPointEntities.Single().LatestValue.Should().Be(dataPointEntryDto.Value);
    }
    
    [Fact]
    public async Task PostDataPointEntry_ReturnsForbidden_WhenInvalidApiKey()
    {
        //Arrange
        await Authenticate();
    
        var dataPointEntryDto =
            new PushDataPointEntryDto("key", 500);
    
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
    
        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"/api/public/v1/DataPointEntries/{IdGenerator.GenerateId()}/single", UriKind.Relative),
                stringContent);
    
        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().BeEmpty();
    }
    
    [Fact]
    public async Task PostNewDataPointEntries_PersistsEntriesInDatabase()
    {
        //Arrange
        var organization = await SetupOrganization();
    
        var dataPointEntryDto =
            new PushDataPointEntryDto("dataPointKey",500);
    
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
    
        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"/api/public/v1/DataPointEntries/{organization.ApiKey}/single", UriKind.Relative),
                stringContent);
    
        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().NotBeNull();
        var dataPointEntryEntity = dataPointEntryEntities.Single();
        dataPointEntryEntity.OrganizationId.Should().Be(organization.Id.ToString());
        dataPointEntryEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
        dataPointEntryEntity.Time.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    
        var dataPointEntities = await GetAll<DataPointEntity>();
        var dataPointEntity = dataPointEntities.Single();
        dataPointEntity.OrganizationId.Should().Be(organization.Id.ToString());
        dataPointEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntity.DisplayName.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntity.ComparisonIsAbsolute.Should().BeFalse();
        dataPointEntity.DirectionIsUp.Should().BeTrue();
        dataPointEntity.LatestValue.Should().Be(dataPointEntryDto.Value);
    }
}