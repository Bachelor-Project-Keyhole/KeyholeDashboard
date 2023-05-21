using System.Net;
using Contracts;
using Domain;
using Domain.Datapoint;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Datapoint;
using Repository.Organization;

namespace WebApi.Tests.IntegrationTests;

public class TemplateControllerTests : IntegrationTest
{
    [Fact]
    public async Task GetDataForTemplate_ReturnsNotFound_WhenDataPointIdIsInvalid()
    {
        //Arrange
        await Authenticate();
        var organization = new OrganizationPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationName = "wow",
            OrganizationOwnerId = "aa",
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };
        
        await PopulateDatabase(new[] {organization});


        var datapointEntities = new[]
        {
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName", latestValue: 60),
            new DataPointEntity(IdGenerator.GenerateId(), "otherKey", "DisplayName", latestValue: 100)
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"api/v1/Template/{organization.Id.ToString()}/{IdGenerator.GenerateId()}",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDataForTemplate_ReturnsOnlyEntriesInTimespanAndCorrectValues()
    {
        //Arrange
        await Authenticate();
        var organization = new OrganizationPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationName = "wow",
            OrganizationOwnerId = "aa",
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };
        
        await PopulateDatabase(new[] {organization});

        var dataPointEntity = new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName", latestValue: 60)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] { dataPointEntity });

        var entitiesToBeReturned = new DataPointEntryEntity[]
        {
            new(organization.Id.ToString(), dataPointEntity.DataPointKey, 10, DateTime.Now),
            new(organization.Id.ToString(), dataPointEntity.DataPointKey, 20, DateTime.Now.AddDays(-2))
        };
        await PopulateDatabase(entitiesToBeReturned);

        var otherEntities = new[]
        {
            new DataPointEntryEntity(organization.Id.ToString(), dataPointEntity.DataPointKey, 23, DateTime.Now.AddDays(-10)),
            new DataPointEntryEntity(organization.Id.ToString(), "other", 23, DateTime.Now),
            new DataPointEntryEntity(IdGenerator.GenerateId(), dataPointEntity.DataPointKey, 23, DateTime.Now),
            new DataPointEntryEntity(IdGenerator.GenerateId(), "random", 23, DateTime.Now),
        };
        await PopulateDatabase(otherEntities);


        var expectedResult = new DataPointEntryDto[]
        {
            new()
            {
                Value = 30,
                Time = entitiesToBeReturned[1].Time
            },
            new()
            {
                Value = 15,
                Time = entitiesToBeReturned[0].Time
            }
        };
        
        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"api/v1/Template/{organization.Id.ToString()}/{dataPointEntity.Id}?timeSpanInDays=5",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointEntryDto[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().NotBeNull();
        result!.Length.Should().Be(2);
        result[0].Value.Should().Be(expectedResult[0].Value);
        result[0].Time.Should().BeCloseTo(expectedResult[0].Time, TimeSpan.FromSeconds(1));
        result[1].Value.Should().Be(expectedResult[1].Value);
        result[1].Time.Should().BeCloseTo(expectedResult[1].Time, TimeSpan.FromSeconds(1));
    }
}