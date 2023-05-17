using System.Net;
using System.Text;
using Contracts;
using Domain;
using Domain.Datapoint;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Datapoint;

namespace WebApi.Tests.IntegrationTests.DataPointController;

public class DataPointControllerTests : IntegrationTest
{
    [Fact]
    public async Task GetLatestDataPointEntry_ReturnsProperValue()
    {
        //Arrange
        var organizationId = await SetupOrganization();

        var key = "TestKey";
        var expectedTime = DateTime.Now;
        var expectedValue = 23.42;

        var dataPointEntryEntities = new DataPointEntryEntity[]
        {
            new(organizationId, key, expectedValue, expectedTime),
            new(organizationId, key, 0, expectedTime.AddDays(-1)),
            new(organizationId, key, 10, expectedTime.AddDays(-2)),
            new(organizationId, key, -50000.25, expectedTime.AddMinutes(-5)),
            new(organizationId, key, 1.563, expectedTime.AddHours(-2)),
        };
        await PopulateDatabase(dataPointEntryEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(
                new Uri($"api/v1/DataPoint/entries/last/{organizationId}/{key}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointEntryDto>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().NotBeNull();
        result!.OrganizationId.Should().Be(organizationId);
        result.DataPointKey.Should().Be(key);
        result.Value.Should().Be(expectedValue);
        result.Time.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetAllDataPointsWithLatestValues_ReturnsAllDataPointsBelongingToOrganization()
    {
        //Arrange
        var organizationId = await SetupOrganization();
        var expectedKeys = new[]
        {
            "testKey1",
            "testKey2"
        };

        var datapointEntities = new[]
        {
            new DataPointEntity(organizationId, expectedKeys[0], "DisplayName", latestValue: 42),
            new DataPointEntity(organizationId, expectedKeys[1], "DisplayName", latestValue: 23),
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName", latestValue: 60),
            new DataPointEntity(IdGenerator.GenerateId(), "otherKey", "DisplayName", latestValue: 100)
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organizationId}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointWithValueDto[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().HaveCount(2);
        var dataPointWithValueDto = result?.First(dto => dto.DataPointKey == expectedKeys[0]);
        dataPointWithValueDto?.LatestValue.Should().Be(42);
        var dataPointWithValueDto2 = result?.First(dto => dto.DataPointKey == expectedKeys[1]);
        dataPointWithValueDto2?.LatestValue.Should().Be(23);
    }

    [Fact]
    public async Task GetAllDataPointsWithLatestValues_ReturnsEmptyArray_WhenNoDataPointsArePresent()
    {
        //Arrange
        var organizationId = await SetupOrganization();
        var datapointEntities = new[]
        {
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName"),
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName"),
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organizationId}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointWithValueDto[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDataPointsWithLatestValues_ReturnsNotFound_WhenOrganizationIdDoesNotExists()
    {
        //Arrange
        await SetupOrganization();

        var nonExistingOrganizationId = IdGenerator.GenerateId();

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{nonExistingOrganizationId}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostDataPointEntry_PersistsEntryIntoDatabase_AndCreatesNewDataPoint()
    {
        //Arrange
        var organizationId = await SetupOrganization();

        var dataPointEntryDto =
            new DataPointEntryDto(organizationId, IdGenerator.GenerateId(), 500, DateTime.Now);

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"/api/v1/DataPoint/entries", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().NotBeNull();
        var dataPointEntryEntity = dataPointEntryEntities.Single();
        dataPointEntryEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
        dataPointEntryEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
        dataPointEntryEntity.Time.Should().BeCloseTo(dataPointEntryEntity.Time, TimeSpan.FromSeconds(1));

        var dataPointEntities = await GetAll<DataPointEntity>();
        var dataPointEntity = dataPointEntities.Single();
        dataPointEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
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
        var organizationId = await SetupOrganization();

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organizationId, key, key);
        await PopulateDatabase(new[] { dataPointEntity });

        var dataPointEntryDto =
            new DataPointEntryDto(organizationId, key, 500, DateTime.Now);

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"/api/v1/DataPoint/entries", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        var dataPointEntryEntity = dataPointEntryEntities.Single();
        dataPointEntryEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
        dataPointEntryEntity.DataPointKey.Should().Be(dataPointEntryDto.DataPointKey);
        dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
        dataPointEntryEntity.Time.Should().BeCloseTo(dataPointEntryEntity.Time, TimeSpan.FromSeconds(1));

        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(1);
        dataPointEntities.Single().LatestValue.Should().Be(dataPointEntryDto.Value);
    }

    [Fact]
    public async Task PostDataPointEntry_ReturnsNotFound_WhenOrganizationIdDoesNotExists()
    {
        //Arrange
        var nonExistingOrganizationId = IdGenerator.GenerateId();

        var dataPointEntryDto =
            new DataPointEntryDto(nonExistingOrganizationId, IdGenerator.GenerateId(), 500, DateTime.Now);

        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri($"/api/v1/DataPoint/entries", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDataPointEntries_ReturnsAllExpectedDataPointEntries()
    {
        //Arrange
        var organizationId = await SetupOrganization();
        var key = "TestKey";

        var expectedEntities = new DataPointEntryEntity[]
        {
            new(organizationId, key, 23, DateTime.Now),
            new(organizationId, key, 23, DateTime.MinValue),
        };

        await PopulateDatabase(expectedEntities);

        var testEntities = new[]
        {
            new DataPointEntryEntity(organizationId, "notTestKey", 23, DateTime.Now),
            new DataPointEntryEntity(IdGenerator.GenerateId(), "other", 23, DateTime.Now),
            new DataPointEntryEntity(IdGenerator.GenerateId(), key, 23, DateTime.Now),
        };
        await PopulateDatabase(testEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/entries/{organizationId}/{key}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointEntryDto[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().NotBeNull();
        AssertDataPointEntries(expectedEntities, result!);
    }

    [Fact]
    public async Task GetAllDataPointEntries_ReturnsNotFound_WhenOrganizationIsNotFound()
    {
        //Arrange
        var key = "TestKey";
        var testEntities = new[]
        {
            new DataPointEntryEntity(IdGenerator.GenerateId(), "notTestKey", 23, DateTime.Now),
            new DataPointEntryEntity(IdGenerator.GenerateId(), "other", 23, DateTime.Now),
            new DataPointEntryEntity(IdGenerator.GenerateId(), key, 23, DateTime.Now),
        };
        await PopulateDatabase(testEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{IdGenerator.GenerateId()}/{key}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllDataPointEntries_ReturnsNotFound_WhenNoEntriesWithMatchingKeyAroundFound()
    {
        //Arrange
        var organizationId = await SetupOrganization();

        var key = "TestKey";
        var testEntities = new[]
        {
            new DataPointEntryEntity(organizationId, "notTestKey", 23, DateTime.Now),
            new DataPointEntryEntity(organizationId, "other", 23, DateTime.Now),
            new DataPointEntryEntity(IdGenerator.GenerateId(), key, 23, DateTime.Now),
        };
        await PopulateDatabase(testEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organizationId}/{key}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDataPoint_UpdatesDataPointWithProperValues()
    {
        //Arrange
        var organizationId = await SetupOrganization();

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organizationId, key, "Old Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        await PopulateDatabase(new[] { dataPointEntity });

        var dataPointEntryEntity =
            new DataPointEntryEntity(organizationId, dataPointEntity.DataPointKey, 10, DateTime.Now);
        await PopulateDatabase(new[] { dataPointEntryEntity });

        var dataPointDto =
            new DataPointDto(dataPointEntity.Id.ToString(), organizationId, key, "New Display Name",
                new FormulaDto() { Operation = MathOperation.Multiply.ToString(), Factor = 1.2 }, false, true);
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PatchAsync(new Uri("api/v1/DataPoint", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedDataPointEntity = GetAll<DataPointEntity>().Result.Single();
        updatedDataPointEntity.LatestValue.Should().Be(12);
        AssertDataPoint(updatedDataPointEntity, dataPointDto);
    }

    private static void AssertDataPoint(DataPointEntity result, DataPointDto expected)
    {
        result.Id.Should().Be(new ObjectId(expected.Id));
        result.OrganizationId.Should().Be(expected.OrganizationId);
        result.DataPointKey.Should().Be(expected.DataPointKey);
        result.DisplayName.Should().Be(expected.DisplayName);
        result.Formula.Operation.Should().Be(Enum.Parse<MathOperation>(expected.Formula.Operation));
        result.Formula.Factor.Should().Be(expected.Formula.Factor);
        result.ComparisonIsAbsolute.Should().Be(expected.ComparisonIsAbsolute);
        result.DirectionIsUp.Should().Be(expected.DirectionIsUp);
    }

    private void AssertDataPointEntries(DataPointEntryEntity[] expected, DataPointEntryDto[] actual)
    {
        actual.Should().HaveCount(expected.Length);
        foreach (var dataPointEntry in actual.Zip(expected, (a, e) => (a, e)))
        {
            dataPointEntry.a.OrganizationId.Should().Be(dataPointEntry.e.OrganizationId);
            dataPointEntry.a.DataPointKey.Should().Be(dataPointEntry.e.DataPointKey);
            dataPointEntry.a.Value.Should().Be(dataPointEntry.e.Value);
            dataPointEntry.a.Time.Should().BeCloseTo(dataPointEntry.e.Time, TimeSpan.FromSeconds(1));
        }
    }
}