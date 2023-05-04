using System.Net;
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
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{nonExistingOrganizationId}",UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task PostDataPoint_PersistsEntryIntoDatabase(){}
    
    
}