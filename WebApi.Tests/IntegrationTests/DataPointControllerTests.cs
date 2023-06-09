using System.Net;
using System.Text;
using Contracts.v1.DataPoint;
using Domain;
using Domain.Datapoint;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Datapoint;

namespace WebApi.Tests.IntegrationTests;

public class DataPointControllerTests : IntegrationTest
{
    [Fact]
    public async Task GetDataPointDisplayNames_ReturnsCorrectValues()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var expectedKeys = new[]
        {
            "testKey1",
            "testKey2"
        };

        var expectedEntities = new[]
        {
            new DataPointEntity(IdGenerator.GenerateId(), organization.Id.ToString(), expectedKeys[0], "DisplayName1",
                latestValue: 42),
            new DataPointEntity(IdGenerator.GenerateId(), organization.Id.ToString(), expectedKeys[1], "DisplayName2",
                latestValue: 23),
            new DataPointEntity(IdGenerator.GenerateId(), organization.Id.ToString(), expectedKeys[1], "DisplayName3",
                latestValue: 23),
        };

        await PopulateDatabase(expectedEntities);

        var datapointEntities = new[]
        {
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName", latestValue: 60),
            new DataPointEntity(IdGenerator.GenerateId(), "otherKey", "DisplayName", latestValue: 100)
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organization.Id.ToString()}/displayNames",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointDisplayNameResponse[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().HaveCount(3);
        result!
            .Select(dp => dp.DisplayName)
            .Should().BeEquivalentTo(expectedEntities.Select(dp => dp.DisplayName));
        result!
            .Select(dp => dp.Id)
            .Should().BeEquivalentTo(expectedEntities.Select(dp => dp.Id.ToString()));
    }

    [Fact]
    public async Task CreateDataPoint_StoresProperInformationInDatabase()
    {
        //Arrange
        await Authenticate();
        var dataPointKey = "TestKey";
        var organization = await SetupOrganization();

        var dataPointEntryEntities = new DataPointEntryEntity[]
        {
            new(organization.Id.ToString(), dataPointKey, 50, DateTime.Now),
            new(organization.Id.ToString(), dataPointKey, 0, DateTime.Now.AddDays(-1)),
        };
        await PopulateDatabase(dataPointEntryEntities);

        var formulaDto = new FormulaDto
        {
            Operation = MathOperation.Multiply.ToString(),
            Factor = 1.5
        };
        var createDataPointDto = new CreateDataPointRequest(organization.Id.ToString(), dataPointKey, "Display Name",
            formulaDto, true, true);
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(createDataPointDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("api/v1/DataPoint", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointDto = JsonConvert.DeserializeObject<DataPointDto>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        var dataPointEntity = GetAll<DataPointEntity>().Result.Single();
        AssertDataPoint(dataPointEntity, dataPointDto!);
    }

    [Fact]
    public async Task CreateDataPoint_ReturnsNotFoundIfOrganizationIsNotPresent()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dataPointKey = "TestKey";

        var dataPointEntryEntities = new DataPointEntryEntity[]
        {
            new(organization.Id.ToString(), dataPointKey, 50, DateTime.Now),
            new(organization.Id.ToString(), dataPointKey, 0, DateTime.Now.AddDays(-1)),
        };
        await PopulateDatabase(dataPointEntryEntities);

        var formulaDto = new FormulaDto
        {
            Operation = MathOperation.Multiply.ToString(),
            Factor = 1.5
        };
        var createDataPointDto = new CreateDataPointRequest(IdGenerator.GenerateId(), dataPointKey, "Display Name",
            formulaDto, true, true);
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(createDataPointDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("api/v1/DataPoint", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetAll<DataPointEntity>().Result.Should().HaveCount(0);
    }

    [Fact]
    public async Task CreateDataPoint_ReturnsNotFoundIfDataPointKeyIsNotPresent()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dataPointKey = "TestKey";

        var dataPointEntryEntities = new DataPointEntryEntity[]
        {
            new(organization.Id.ToString(), dataPointKey, 50, DateTime.Now),
            new(organization.Id.ToString(), dataPointKey, 0, DateTime.Now.AddDays(-1)),
        };
        await PopulateDatabase(dataPointEntryEntities);

        var formulaDto = new FormulaDto
        {
            Operation = MathOperation.Multiply.ToString(),
            Factor = 1.5
        };
        var createDataPointDto = new CreateDataPointRequest(organization.Id.ToString(), "Nope", "Display Name",
            formulaDto, true, true);
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(createDataPointDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("api/v1/DataPoint", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetAll<DataPointEntity>().Result.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetAllDataPointsWithLatestValues_ReturnsAllDataPointsBelongingToOrganization()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var expectedKeys = new[]
        {
            "testKey1",
            "testKey2"
        };

        var datapointEntities = new[]
        {
            new DataPointEntity(organization.Id.ToString(), expectedKeys[0], "DisplayName", latestValue: 42),
            new DataPointEntity(organization.Id.ToString(), expectedKeys[1], "DisplayName", latestValue: 23),
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName", latestValue: 60),
            new DataPointEntity(IdGenerator.GenerateId(), "otherKey", "DisplayName", latestValue: 100)
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organization.Id.ToString()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointDto[]>(
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
        await Authenticate();

        var organization = await SetupOrganization();

        var datapointEntities = new[]
        {
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName"),
            new DataPointEntity(IdGenerator.GenerateId(), "key", "DisplayName"),
        };
        await PopulateDatabase(datapointEntities);

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{organization.Id.ToString()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonConvert.DeserializeObject<DataPointDto[]>(
            await httpResponseMessage.Content.ReadAsStringAsync());
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDataPointsWithLatestValues_ReturnsNotFound_WhenOrganizationIdDoesNotExists()
    {
        //Arrange
        await Authenticate();

        var organization = await SetupOrganization();

        var nonExistingOrganizationId = IdGenerator.GenerateId();

        //Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/DataPoint/{nonExistingOrganizationId}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDataPoint_UpdatesDataPointWithProperValues()
    {
        //Arrange
        await Authenticate();

        var organization = await SetupOrganization();

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), key, "Old Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        await PopulateDatabase(new[] { dataPointEntity });

        var dataPointEntryEntity =
            new DataPointEntryEntity(organization.Id.ToString(), dataPointEntity.DataPointKey, 10, DateTime.Now);
        await PopulateDatabase(new[] { dataPointEntryEntity });

        var formulaDto = new FormulaDto
        {
            Operation = MathOperation.Multiply.ToString(),
            Factor = 1.2
        };

        var dataPointDto =
            new DataPointDto(dataPointEntity.Id.ToString(), organization.Id.ToString(), key, "New Display Name",
                formulaDto, 0, false, true);
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

    [Fact]
    public async Task UpdateDataPoint_ReturnsNotFound_WhenOrganizationIsNotFound()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), key, "Old Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        await PopulateDatabase(new[] { dataPointEntity });

        var dataPointEntryEntity =
            new DataPointEntryEntity(organization.Id.ToString(), dataPointEntity.DataPointKey, 10, DateTime.Now);
        await PopulateDatabase(new[] { dataPointEntryEntity });

        var formulaDto = new FormulaDto
        {
            Operation = MathOperation.Multiply.ToString(),
            Factor = 1.2
        };

        var dataPointDto =
            new DataPointDto(dataPointEntity.Id.ToString(), IdGenerator.GenerateId(), key, "New Display Name",
                formulaDto, 0, false, true);
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PatchAsync(new Uri("api/v1/DataPoint", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var updatedDataPointEntity = GetAll<DataPointEntity>().Result.Single();
        updatedDataPointEntity.Should().BeEquivalentTo(dataPointEntity);
    }

    [Fact]
    public async Task UpdateDataPoint_ReturnsNotFound_WhenDataPointKeyIsNotFound()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), key, "Old Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        await PopulateDatabase(new[] { dataPointEntity });

        var dataPointEntryEntity =
            new DataPointEntryEntity(organization.Id.ToString(), dataPointEntity.DataPointKey, 10, DateTime.Now);
        await PopulateDatabase(new[] { dataPointEntryEntity });

        var formulaDto = new FormulaDto
        {
            Operation = MathOperation.Multiply.ToString(),
            Factor = 1.2
        };

        var dataPointDto =
            new DataPointDto(dataPointEntity.Id.ToString(), organization.Id.ToString(), "Nope", "New Display Name",
                formulaDto, 0, false, true);
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PatchAsync(new Uri("api/v1/DataPoint", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var updatedDataPointEntity = GetAll<DataPointEntity>().Result.Single();
        updatedDataPointEntity.Should().BeEquivalentTo(dataPointEntity);
    }

    [Fact]
    public async Task UpdateDataPoint_DoesNotUpdateAnything_WhenDataPointIdIsNotFound()
    {
        //Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), key, "Old Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        await PopulateDatabase(new[] { dataPointEntity });

        var dataPointEntryEntity =
            new DataPointEntryEntity(organization.Id.ToString(), dataPointEntity.DataPointKey, 10, DateTime.Now);
        await PopulateDatabase(new[] { dataPointEntryEntity });

        var formulaDto = new FormulaDto
        {
            Operation = MathOperation.Multiply.ToString(),
            Factor = 1.2
        };

        var dataPointDto =
            new DataPointDto(IdGenerator.GenerateId(), organization.Id.ToString(), key, "New Display Name",
                formulaDto, 0, false, true);
        var stringContent =
            new StringContent(JsonConvert.SerializeObject(dataPointDto), Encoding.UTF8, "application/json");

        //Act
        var httpResponseMessage =
            await TestClient.PatchAsync(new Uri("api/v1/DataPoint", UriKind.Relative), stringContent);

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedDataPointEntity = GetAll<DataPointEntity>().Result.Single();
        updatedDataPointEntity.Should().BeEquivalentTo(dataPointEntity);
    }

    [Fact]
    public async Task DeleteDataPoint_ReturnsNotFound_WhenInvalidDataPointId()
    {
        //Arrange
        await Authenticate();
        var organization = SetupOrganization();

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), key, "Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        await PopulateDatabase(new[] { dataPointEntity });

        //Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"api/v1/DataPoint/{IdGenerator.GenerateId()}?forceDelete=false",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteDataPoint_ReturnsConflict_WhenLastKeyIsToBeDeleted_AndNoForceDelete()
    {
        //Arrange
        await Authenticate();
        var organization = SetupOrganization();

        var key = "TestKey";
        var dataPointEntity = new DataPointEntity(organization.Id.ToString(), key, "Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        await PopulateDatabase(new[] { dataPointEntity });

        //Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"api/v1/DataPoint/{dataPointEntity.Id}?forceDelete=false",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteDataPoint_DeletesDataPointAndEntries_WhenLastKeyToBeDeleted_AndForceDelete()
    {
        //Arrange
        await Authenticate();
        var organization = SetupOrganization();

        var key = "TestKey";
        var dataPointEntityToDelete = new DataPointEntity(organization.Id.ToString(), key, "Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        

        var otherDataPointEntity1 = new DataPointEntity(organization.Id.ToString(), "qwe", "Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };

        await PopulateDatabase(new[] { dataPointEntityToDelete, otherDataPointEntity1 });
        
        var dataPointEntries = new DataPointEntryEntity[]
        {
            new(organization.Id.ToString(), dataPointEntityToDelete.DataPointKey, 10, DateTime.UtcNow),
            new(organization.Id.ToString(), otherDataPointEntity1.DataPointKey, 20, DateTime.UtcNow.AddDays(-2)),
            new(IdGenerator.GenerateId(), dataPointEntityToDelete.DataPointKey, 20, DateTime.UtcNow.AddDays(-3))
        };
        await PopulateDatabase(dataPointEntries);
        
        //Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"api/v1/DataPoint/{dataPointEntityToDelete.Id}?forceDelete=true",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(1);
        dataPointEntities.Should()
            .NotContain(dataPointEntityToDelete)
            .And.ContainEquivalentOf(otherDataPointEntity1);
        var entries = await GetAll<DataPointEntryEntity>();
        entries.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteDataPoint_DeletesDataPointAndNotEntries_WhenMoreDataPointsExist()
    {
        //Arrange
        await Authenticate();
        var organization = SetupOrganization();

        var key = "TestKey";
        var dataPointEntityToDelete = new DataPointEntity(organization.Id.ToString(), key, "Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        
        var otherDataPointEntity1 = new DataPointEntity(organization.Id.ToString(), key, "Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };
        
        var otherDataPointEntity2 = new DataPointEntity(organization.Id.ToString(), "qwe", "Display Name")
        {
            Id = new ObjectId(IdGenerator.GenerateId()),
            Formula = new Formula { Operation = MathOperation.Divide, Factor = 2.3 },
            LatestValue = 10
        };

        await PopulateDatabase(new[] { dataPointEntityToDelete, otherDataPointEntity1, otherDataPointEntity2 });
        
        var dataPointEntries = new DataPointEntryEntity[]
        {
            new(organization.Id.ToString(), dataPointEntityToDelete.DataPointKey, 10, DateTime.UtcNow),
            new(organization.Id.ToString(), otherDataPointEntity1.DataPointKey, 20, DateTime.UtcNow.AddDays(-2)),
            new(IdGenerator.GenerateId(), otherDataPointEntity2.DataPointKey, 20, DateTime.UtcNow.AddDays(-3))
        };
        await PopulateDatabase(dataPointEntries);
        
        //Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"api/v1/DataPoint/{dataPointEntityToDelete.Id}?forceDelete=true",
                UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var dataPointEntities = await GetAll<DataPointEntity>();
        dataPointEntities.Should().HaveCount(2);
        dataPointEntities.Should()
            .NotContain(dataPointEntityToDelete)
            .And.ContainEquivalentOf(otherDataPointEntity1)
            .And.ContainEquivalentOf(otherDataPointEntity2);
        var entries = await GetAll<DataPointEntryEntity>();
        entries.Should().HaveCount(3);
    }

    private static void AssertDataPoint(DataPointEntity result, DataPointDto expected)
    {
        result.Id.Should().Be(new ObjectId(expected.Id));
        result.OrganizationId.Should().Be(expected.OrganizationId);
        result.DataPointKey.Should().Be(expected.DataPointKey);
        result.DisplayName.Should().Be(expected.DisplayName);
        result.Formula.Operation.Should().Be(Enum.Parse<MathOperation>(expected.Formula.Operation));
        result.Formula.Factor.Should().Be(expected.Formula.Factor);
        result.IsComparisonAbsolute.Should().Be(expected.IsComparisonAbsolute);
        result.IsDirectionUp.Should().Be(expected.IsDirectionUp);
    }
}