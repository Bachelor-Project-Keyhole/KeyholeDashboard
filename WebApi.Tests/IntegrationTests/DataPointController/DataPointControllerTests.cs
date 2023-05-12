using System.Net;
using System.Text;
using Contracts;
using Domain;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Datapoint;
using Repository.Organization;

namespace WebApi.Tests.IntegrationTests.DataPointController;

public class DataPointControllerTests : IntegrationTest
{
    
    [Fact]
    public async Task GetAllDataPoints_ReturnsAllDataPointsBelongingToOrganization()
    {
        //Arrange
        var organizationId = IdGenerator.GenerateId();
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});
        var datapointEntities = new[]
        {
            new DataPointEntity(organizationId, "key", "DisplayName"),
            new DataPointEntity(organizationId, "key", "DisplayName"),
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName"),
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName")
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organizationId}",UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointDto[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetAllDataPoints_ReturnsEmptyArray_WhenNoDataPointsArePresent()
    {
        //Arrange
        var organizationId = IdGenerator.GenerateId();
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});
        var datapointEntities = new[]
        {
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName"),
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName"),
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organizationId}",UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointDto[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllDataPoints_ReturnsNotFound_WhenOrganizationIdDoesNotExists()
    {
        //Arrange
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});
        var nonExistingOrganizationId = IdGenerator.GenerateId();

        //Act
        var httpResponseMessage = 
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{nonExistingOrganizationId}",UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostDataPointEntry_PersistsEntryIntoDatabase_AndCreatesNewDataPoint()
    {
        //Arrange
        var organizationId = IdGenerator.GenerateId();
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});

        var dataPointEntryDto = 
            new DataPointEntryDto(organizationId, IdGenerator.GenerateId(), 500, DateTime.Now);
        
        var stringContent = 
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
        
        //Act
        var httpResponseMessage = 
            await TestClient.PostAsync(new Uri($"/api/v1/DataPoint/entries",UriKind.Relative), stringContent);
        
        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().NotBeNull();
        var dataPointEntryEntity = dataPointEntryEntities.Single();
        dataPointEntryEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
        dataPointEntryEntity.Key.Should().Be(dataPointEntryDto.Key);
        dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
        dataPointEntryEntity.Time.Should().BeCloseTo(dataPointEntryEntity.Time, TimeSpan.FromSeconds(1));

        var dataPointEntities = await GetAll<DataPointEntity>();
        var dataPointEntity = dataPointEntities.Single();
        dataPointEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
        dataPointEntity.Key.Should().Be(dataPointEntryDto.Key);
        dataPointEntity.DisplayName.Should().Be(dataPointEntryDto.Key);
        dataPointEntity.ComparisonIsAbsolute.Should().BeFalse();
        dataPointEntity.DirectionIsUp.Should().BeTrue();
    }
    
    [Fact]
    public async Task PostDataPointEntry_PersistsEntryIntoDatabase_DoesNotCreateDuplicateDataPoint()
    {
        //Arrange
        var organizationId = IdGenerator.GenerateId();
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organizationId, key, key);
        await PopulateDatabase(new[] { dataPointEntity });

        var dataPointEntryDto = 
            new DataPointEntryDto(organizationId, key, 500, DateTime.Now);
        
        var stringContent = 
            new StringContent(JsonConvert.SerializeObject(dataPointEntryDto), Encoding.UTF8, "application/json");
        
        //Act
        var httpResponseMessage = 
            await TestClient.PostAsync(new Uri($"/api/v1/DataPoint/entries",UriKind.Relative), stringContent);
        
        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        var dataPointEntryEntity = dataPointEntryEntities.Single();
        dataPointEntryEntity.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
        dataPointEntryEntity.Key.Should().Be(dataPointEntryDto.Key);
        dataPointEntryEntity.Value.Should().Be(dataPointEntryDto.Value);
        dataPointEntryEntity.Time.Should().BeCloseTo(dataPointEntryEntity.Time, TimeSpan.FromSeconds(1));

        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(1);
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
            await TestClient.PostAsync(new Uri($"/api/v1/DataPoint/entries",UriKind.Relative), stringContent);
        
        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dataPointEntryEntities = await GetAll<DataPointEntryEntity>();
        dataPointEntryEntities.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllDataPointEntries_ReturnsAllExpectedDataPointEntries()
    {
        //Arrange
        var organizationId = IdGenerator.GenerateId();
        var key = "TestKey";
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});
        
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
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organizationId}/{key}",UriKind.Relative));

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
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{IdGenerator.GenerateId()}/{key}",UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetAllDataPointEntries_ReturnsNotFound_WhenNoEntriesWithMatchingKeyAroundFound()
    {
        //Arrange
        var organizationId = IdGenerator.GenerateId();
        var key = "TestKey";
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});
        
        var testEntities = new[]
        {
            new DataPointEntryEntity(organizationId, "notTestKey", 23, DateTime.Now),
            new DataPointEntryEntity(organizationId, "other", 23, DateTime.Now),
            new DataPointEntryEntity(IdGenerator.GenerateId(), key, 23, DateTime.Now),
        };
        await PopulateDatabase(testEntities);

        //Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organizationId}/{key}",UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDataPoint_UpdatesDataPointWithProperValues()
    {
        //Arrange
        var organizationId = IdGenerator.GenerateId();
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organizationId, key, "Old Display Name");
        await PopulateDatabase(new[] { dataPointEntity });

        var dataPointDto = 
            new DataPointDto(IdGenerator.GenerateId(), organizationId, key, "New Display Name", false, true);
        var stringContent = 
            new StringContent(JsonConvert.SerializeObject(dataPointDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage = await TestClient.PatchAsync(new Uri("api/v1/DataPoint", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedDataPointEntity = GetAll<DataPointEntity>().Result.Single();
        AssertDataPoint(updatedDataPointEntity, dataPointDto);
    }

    private static void AssertDataPoint(DataPointEntity result, DataPointDto expected)
    {
        result.OrganizationId.Should().Be(expected.OrganizationId);
        result.Key.Should().Be(expected.Key);
        result.DisplayName.Should().Be(expected.DisplayName);
        result.ComparisonIsAbsolute.Should().Be(expected.ComparisonIsAbsolute);
        result.DirectionIsUp.Should().Be(expected.DirectionIsUp);
    }

    private void AssertDataPointEntries(DataPointEntryEntity[] expected, DataPointEntryDto[] actual)
    {
        actual.Should().HaveCount(expected.Length);
        foreach (var dataPointEntry in actual.Zip(expected, (a,e) => (a, e)))
        {
            dataPointEntry.a.OrganizationId.Should().Be(dataPointEntry.e.OrganizationId);
            dataPointEntry.a.Key.Should().Be(dataPointEntry.e.Key);
            dataPointEntry.a.Value.Should().Be(dataPointEntry.e.Value);
            dataPointEntry.a.Time.Should().BeCloseTo(dataPointEntry.e.Time, TimeSpan.FromSeconds(1));
        }
    }
}