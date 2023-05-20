using System.Net;
using Contracts;
using Domain;
using Domain.Datapoint;
using FluentAssertions;
using Newtonsoft.Json;
using Repository.Datapoint;

namespace WebApi.Tests.IntegrationTests;

public class TemplateControllerTests : IntegrationTest
{
    [Fact]
    public async Task GetDataForTemplate_ReturnsNotFound_WhenDataPointIdIsInvalid()
    {
        //Arrange
        var organizationId = await SetupOrganization();

        var datapointEntities = new[]
        {
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName", latestValue: 60),
            new DataPointEntity(IdGenerator.GenerateId(), "otherKey", "DisplayName", latestValue: 100)
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"api/v1/Template/{organizationId}/{IdGenerator.GenerateId()}",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDataForTemplate_ReturnsOnlyEntriesInTimespanAndCorrectValues()
    {
        //Arrange
        var organizationId = await SetupOrganization();

        var dataPointEntity = new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName", latestValue: 60)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] { dataPointEntity });

        var entitiesToBeReturned = new DataPointEntryEntity[]
        {
            new(organizationId, dataPointEntity.DataPointKey, 10, DateTime.Now),
            new(organizationId, dataPointEntity.DataPointKey, 20, DateTime.Now.AddDays(-2))
        };
        await PopulateDatabase(entitiesToBeReturned);

        var otherEntities = new[]
        {
            new DataPointEntryEntity(organizationId, dataPointEntity.DataPointKey, 23, DateTime.Now.AddDays(-10)),
            new DataPointEntryEntity(organizationId, "other", 23, DateTime.Now),
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
            await TestClient.GetAsync(new Uri($"api/v1/Template/{organizationId}/{dataPointEntity.Id}?timeSpanInDays=5",
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