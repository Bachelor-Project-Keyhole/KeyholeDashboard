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
        var organizationEntity = new OrganizationEntity()
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});
        var datapointEntities = new[]
        {
            new DatapointEntity(organizationId, "key", 23),
            new DatapointEntity(organizationId, "key", 23),
            new DatapointEntity(IdGenerator.GenerateId(), "key", 23),
            new DatapointEntity(IdGenerator.GenerateId(), "key", 23),
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
        var organizationEntity = new OrganizationEntity()
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});
        var datapointEntities = new[]
        {
            new DatapointEntity(IdGenerator.GenerateId(), "key", 23),
            new DatapointEntity(IdGenerator.GenerateId(), "key", 23),
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
        var organizationEntity = new OrganizationEntity()
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
    public async Task PostDataPoint_PersistsEntryIntoDatabase()
    {
        //Arrange
        var organizationId = IdGenerator.GenerateId();
        var organizationEntity = new OrganizationEntity()
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
            await TestClient.PostAsync(new Uri("/api/v1/DataPoint/Entries",UriKind.Relative), stringContent);
        
        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        httpResponseMessage = 
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organizationId}",UriKind.Relative));
        var result = JsonConvert.DeserializeObject<DataPointDto[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().NotBeNull();
        var dataPointDto = result!.Single();
        dataPointDto.OrganizationId.Should().Be(dataPointEntryDto.OrganizationId);
        dataPointDto.Key.Should().Be(dataPointEntryDto.Key);
        dataPointDto.Value.Should().Be(dataPointEntryDto.Value);
        dataPointDto.Time.Should().Be(dataPointEntryDto.Time);
    }
}