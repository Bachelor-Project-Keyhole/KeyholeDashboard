using System.Net;
using Contracts;
using Domain;
using Domain.Datapoint;
using Domain.Template;
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
        await Authenticate();
        var organization = await SetupOrganization();


        var datapointEntities = new[]
        {
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName", latestValue: 60),
            new DataPointEntity(IdGenerator.GenerateId(), "otherKey", "DisplayName", latestValue: 100)
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri(
                $"api/v1/Template/{organization.Id.ToString()}/{IdGenerator.GenerateId()}",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDataForTemplate_ReturnsOnlyEntriesInTimespanAndCorrectValues()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), "key", "DisplayName", latestValue: 60)
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
            new DataPointEntryEntity(organization.Id.ToString(), dataPointEntity.DataPointKey, 23,
                DateTime.Now.AddDays(-10)),
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
            await TestClient.GetAsync(new Uri(
                $"api/v1/Template/{organization.Id.ToString()}/{dataPointEntity.Id}?timePeriod=5&timeUnit={TimeUnit.Day}",
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

    [Fact]
    public async Task GetLatestValueWithChange_ReturnsZeroWhenNoEntries()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), "key", "DisplayName", latestValue: 60)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] { dataPointEntity });

        //Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Template/latest-value-with-change/{dataPointEntity.Id}?timePeriod=50&timeUnit={TimeUnit.Day}",
            UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<LatestValueWithChangeDto>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result!.LatestValue.Should().Be(dataPointEntity.LatestValue);
        result.Change.Should().Be(0);
    }

    [Fact]
    public async Task GetLatestValueWithChange_ReturnsCorrectResult()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), "key", "DisplayName", latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] { dataPointEntity });

        var timeSpanInDays = 10;
        var expectedSelection = new DataPointEntryEntity(organization.Id.ToString(), dataPointEntity.DataPointKey, 50,
            DateTime.Now.AddDays(-timeSpanInDays).AddHours(-2));
        await PopulateDatabase(new[] { expectedSelection });

        var tesEntities = new DataPointEntryEntity[]
        {
            new(organization.Id.ToString(), dataPointEntity.DataPointKey, 10, DateTime.Now),
            new(organization.Id.ToString(), dataPointEntity.DataPointKey, 0, DateTime.Now.AddDays(-1)),
            new(organization.Id.ToString(), dataPointEntity.DataPointKey, -33, DateTime.Now.AddDays(-5)),
            new(organization.Id.ToString(), dataPointEntity.DataPointKey, 800.45,
                DateTime.Now.AddDays(timeSpanInDays + 2)),
            new(organization.Id.ToString(), "other", 23, DateTime.Now.AddDays(-23)),
            new(IdGenerator.GenerateId(), dataPointEntity.DataPointKey, 23, DateTime.Now.AddDays(-23)),
            new(IdGenerator.GenerateId(), "random", -500, DateTime.Now.AddDays(-23))
        };
        await PopulateDatabase(tesEntities);

        //Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri(
            $"api/v1/Template/latest-value-with-change/{dataPointEntity.Id.ToString()}?timePeriod={timeSpanInDays}&timeUnit={TimeUnit.Day}",
            UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<LatestValueWithChangeDto>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result!.LatestValue.Should().Be(dataPointEntity.LatestValue);
        Math.Round(result.Change, 2).Should().Be(100);
    }
}