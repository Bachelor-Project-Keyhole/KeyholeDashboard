using System.Net;
using System.Text;
using Domain;
using Domain.Datapoint;
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
        var organization = await SetupOrganization();

        var dataPointEntryDto =
            new PushDataPointEntryRequest(IdGenerator.GenerateId(), 500);

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(
                new Uri($"/api/public/v1/DataPointEntries/{organization.ApiKey}/single", UriKind.Relative),
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
            new PushDataPointEntryRequest(key, 500);

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(
                new Uri($"/api/public/v1/DataPointEntries/{organization.ApiKey}/single", UriKind.Relative),
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
        var dataPointEntryDto =
            new PushDataPointEntryRequest("key", 500);

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(
                new Uri($"/api/public/v1/DataPointEntries/{IdGenerator.GenerateId()}/single", UriKind.Relative),
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
            new PushDataPointEntryRequest("dataPointKey", 500);

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(
                new Uri($"/api/public/v1/DataPointEntries/{organization.ApiKey}/single", UriKind.Relative),
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
    public async Task PostDataPointEntries_PersistsEntriesInDatabase_AndCreatesNewDataPointsForMissingKeys()
    {
        //Arrange
        var organization = await SetupOrganization();
        var dataPoint = new DataPointEntity(organization.Id.ToString(), "key", "key")
        {
            Formula = new Formula(MathOperation.Multiply, 2)
        };
        await PopulateDatabase(new[] { dataPoint });

        var dataPointEntryDtos = new[]
        {
            new PushDataPointEntryRequest(dataPoint.DataPointKey, 42),
            new PushDataPointEntryRequest(IdGenerator.GenerateId(), 23)
        };

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDtos), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(
                new Uri($"/api/public/v1/DataPointEntries/{organization.ApiKey}", UriKind.Relative),
                stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().NotBeNull();

        for (int i = 0; i < dataPointEntryEntities.Length; i++)
        {
            var entity = dataPointEntryEntities[i];
            var dto = dataPointEntryDtos[i];

            entity.OrganizationId.Should().Be(organization.Id.ToString());
            entity.DataPointKey.Should().Be(dto.DataPointKey);
            entity.Value.Should().Be(dto.Value);
            entity.Time.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        }

        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(2);
        var dataPointEntity = dataPointEntities.Single(dp => dp.DataPointKey == dataPoint.DataPointKey);
        dataPointEntity.LatestValue.Should().Be(84);
        var dataPointEntity2 = dataPointEntities.Single(dp => dp.DataPointKey == dataPointEntryDtos[1].DataPointKey);
        dataPointEntity2.LatestValue.Should().Be(dataPointEntryDtos[1].Value);
    }
    
    [Fact]
    public async Task PostHistoricDataPointEntries_PersistsEntriesInDatabase_DoesNotUpdateLatestValue()
    {
        //Arrange
        var organization = await SetupOrganization();
        var dataPoint = new DataPointEntity(organization.Id.ToString(), "key", "key")
        {
            Formula = new Formula(MathOperation.Multiply, 2),
            LatestValue = 110
        };
        await PopulateDatabase(new[] { dataPoint });

        var dataPointEntryDtos = new[]
        {
            new HistoricDataPointEntryRequest(dataPoint.DataPointKey, 42, DateTime.UtcNow.AddDays(-7)),
            new HistoricDataPointEntryRequest(dataPoint.DataPointKey, 30, DateTime.UtcNow.AddDays(-8)),
            new HistoricDataPointEntryRequest(dataPoint.DataPointKey, 17, DateTime.UtcNow.AddDays(-9)),
            new HistoricDataPointEntryRequest(IdGenerator.GenerateId(), 23, DateTime.UtcNow.AddDays(-10))
        };

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDtos), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(
                new Uri($"/api/public/v1/DataPointEntries/{organization.ApiKey}/historic", UriKind.Relative),
                stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().NotBeNull();
        dataPointEntryEntities.Length.Should().Be(dataPointEntryDtos.Length);
        for (int i = 0; i < dataPointEntryEntities.Length; i++)
        {
            var entity = dataPointEntryEntities[i];
            var dto = dataPointEntryDtos[i];

            entity.OrganizationId.Should().Be(organization.Id.ToString());
            entity.DataPointKey.Should().Be(dto.DataPointKey);
            entity.Value.Should().Be(dto.Value);
            entity.Time.Should().BeCloseTo(dto.Time, TimeSpan.FromMinutes(5));
        }

        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(2);
        var dataPointEntity = dataPointEntities.Single(dp => dp.DataPointKey == dataPoint.DataPointKey);
        dataPointEntity.LatestValue.Should().Be(110);
        var dataPointEntity2 = dataPointEntities.Single(dp => dp.DataPointKey == dataPointEntryDtos[3].DataPointKey);
        dataPointEntity2.LatestValue.Should().Be(23);
    }
}