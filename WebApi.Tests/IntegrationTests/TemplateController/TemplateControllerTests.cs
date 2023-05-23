using System.Net;
using System.Text;
using Contracts;
using Contracts.Template;
using Domain;
using Domain.Datapoint;
using Domain.Template;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Dashboard;
using Repository.Datapoint;
using Repository.Template;

namespace WebApi.Tests.IntegrationTests.TemplateController;

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
        await PopulateDatabase(new[] {dataPointEntity});

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
        await PopulateDatabase(new[] {dataPointEntity});

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
        result.DirectionIsUp.Should().Be(dataPointEntity.DirectionIsUp);
        result.ComparisonIsAbsolute.Should().Be(dataPointEntity.ComparisonIsAbsolute);
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
        await PopulateDatabase(new[] {dataPointEntity});

        var timeSpanInDays = 10;
        var expectedSelection = new DataPointEntryEntity(organization.Id.ToString(), dataPointEntity.DataPointKey, 50,
            DateTime.Now.AddDays(-timeSpanInDays).AddHours(-2));
        await PopulateDatabase(new[] {expectedSelection});

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
        result.DirectionIsUp.Should().Be(dataPointEntity.DirectionIsUp);
        result.ComparisonIsAbsolute.Should().Be(dataPointEntity.ComparisonIsAbsolute);
    }

    [Fact]
    public async Task CreateTemplate_Successful()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "dashboard",
            OrganizationId = organization.Id.ToString()
        };
        await PopulateDatabase(new[] {dashboardPersistence});

        var dataPointEntity = new DataPointEntity(
            organization.Id.ToString(),
            "key",
            "DisplayName",
            latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] {dataPointEntity});

        var request = new CreateTemplateRequest
        {
            DashboardId = dashboardPersistence.Id.ToString(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Template", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<TemplateResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
        var templates = await GetAll<TemplatePersistenceModel>();
        var template = templates.Single(x => x.Id == ObjectId.Parse(response?.Id));
        template.DashboardId.Should().Be(dashboardPersistence.Id.ToString());
        template.DatapointId.Should().Be(dataPointEntity.Id.ToString());
        template.DisplayType.Should().Be(request.DisplayType);
        template.TimeUnit.Should().Be(request.TimeUnit);
        template.TimePeriod.Should().Be(request.TimePeriod);
        template.SizeHeight.Should().Be(request.SizeHeight);
        template.SizeWidth.Should().Be(request.SizeWidth);
        template.PositionHeight.Should().Be(request.PositionHeight);
        template.PositionWidth.Should().Be(request.PositionWidth);
    }

    [Fact]
    public async Task CreateTemplate_Dashboard_NotFound()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "dashboard",
            OrganizationId = organization.Id.ToString()
        };
        await PopulateDatabase(new[] {dashboardPersistence});

        var dataPointEntity = new DataPointEntity(
            organization.Id.ToString(),
            "key",
            "DisplayName",
            latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] {dataPointEntity});

        var request = new CreateTemplateRequest
        {
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Template", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTemplate_DataPoint_NotFound()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "dashboard",
            OrganizationId = organization.Id.ToString()
        };
        await PopulateDatabase(new[] {dashboardPersistence});

        var dataPointEntity = new DataPointEntity(
            organization.Id.ToString(),
            "key",
            "DisplayName",
            latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] {dataPointEntity});

        var request = new CreateTemplateRequest
        {
            DashboardId = dashboardPersistence.Id.ToString(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Template", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1, 1, 1, 1)]
    [InlineData(1, -1, 1, 1)]
    [InlineData(1, 1, 0, 1)]
    [InlineData(-1, 1, 1, 0)]
    public async Task CreateTemplate_MetricValidationException(int positionH, int positionW, int sizeH, int sizeW)
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "dashboard",
            OrganizationId = organization.Id.ToString()
        };
        await PopulateDatabase(new[] {dashboardPersistence});

        var dataPointEntity = new DataPointEntity(
            organization.Id.ToString(),
            "key",
            "DisplayName",
            latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] {dataPointEntity});

        var request = new CreateTemplateRequest
        {
            DashboardId = dashboardPersistence.Id.ToString(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = positionH,
            PositionWidth = positionW,
            SizeHeight = sizeH,
            SizeWidth = sizeW
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Template", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Should().BeEmpty();
    }


    [Fact]
    public async Task UpdateTemplate_Successful()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();


        var dataPointEntity = new DataPointEntity(
            organization.Id.ToString(),
            "key",
            "DisplayName",
            latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] {dataPointEntity});

        var templatePersistence = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 0,
            PositionWidth = 0,
            SizeHeight = 0,
            SizeWidth = 0
        };
        await PopulateDatabase(new[] {templatePersistence});

        var request = new UpdateTemplateRequest()
        {
            Id = templatePersistence.Id.ToString(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PutAsync(new Uri("/api/v1/Template", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<TemplateResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
        var templates = await GetAll<TemplatePersistenceModel>();
        var template = templates.Single(x => x.Id == ObjectId.Parse(response?.Id));
        template.DashboardId.Should().Be(templatePersistence.DashboardId);
        template.DatapointId.Should().Be(dataPointEntity.Id.ToString());
        template.DisplayType.Should().Be(request.DisplayType);
        template.TimeUnit.Should().Be(request.TimeUnit);
        template.TimePeriod.Should().Be(request.TimePeriod);
        template.SizeHeight.Should().Be(request.SizeHeight);
        template.SizeWidth.Should().Be(request.SizeWidth);
        template.PositionHeight.Should().Be(request.PositionHeight);
        template.PositionWidth.Should().Be(request.PositionWidth);
    }

    [Fact]
    public async Task UpdateTemplate_TemplateNotFoundException()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();


        var dataPointEntity = new DataPointEntity(
            organization.Id.ToString(),
            "key",
            "DisplayName",
            latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] {dataPointEntity});



        var request = new UpdateTemplateRequest()
        {
            Id = IdGenerator.GenerateId(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PutAsync(new Uri("/api/v1/Template", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateTemplate_DataPointNotFound()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();


        var dataPointEntity = new DataPointEntity(
            organization.Id.ToString(),
            "key",
            "DisplayName",
            latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] {dataPointEntity});

        var templatePersistence = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 0,
            PositionWidth = 0,
            SizeHeight = 0,
            SizeWidth = 0
        };
        await PopulateDatabase(new[] {templatePersistence});

        var request = new UpdateTemplateRequest()
        {
            Id = IdGenerator.GenerateId(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PutAsync(new Uri("/api/v1/Template", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Length.Should().Be(1);
    }

    [Theory]
    [InlineData(-1, 1, 1, 1)]
    [InlineData(1, -1, 1, 1)]
    [InlineData(1, 1, 0, 1)]
    [InlineData(-1, 1, 1, 0)]
    public async Task UpdateTemplate_MetricValidationException(int positionH, int positionW, int sizeH, int sizeW)
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();


        var dataPointEntity = new DataPointEntity(
            organization.Id.ToString(),
            "key",
            "DisplayName",
            latestValue: 150)
        {
            Formula = new Formula(MathOperation.Multiply, 1.5)
        };
        await PopulateDatabase(new[] {dataPointEntity});

        var templatePersistence = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };
        await PopulateDatabase(new[] {templatePersistence});

        var request = new UpdateTemplateRequest
        {
            Id = templatePersistence.Id.ToString(),
            DatapointId = dataPointEntity.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = positionH,
            PositionWidth = positionW,
            SizeHeight = sizeH,
            SizeWidth = sizeW
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PutAsync(new Uri("/api/v1/Template", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Length.Should().Be(1);
    }

    [Fact]
    public async Task GetTemplateById_Successful()
    {
        // Arrange
        await Authenticate();
        var templatePersistence = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };
        await PopulateDatabase(new[] {templatePersistence});

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(
                new Uri($"/api/v1/Template/{templatePersistence.Id.ToString()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<TemplateResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
        var templates = await GetAll<TemplatePersistenceModel>();
        var template = templates.Single(x => x.Id == ObjectId.Parse(response?.Id));
        response?.Id.Should().Be(templatePersistence.Id.ToString());
        response?.DashboardId.Should().Be(templatePersistence.DashboardId);
        response?.DatapointId.Should().Be(templatePersistence.DatapointId);
        response?.DisplayType.Should().Be(templatePersistence.DisplayType);
        response?.TimePeriod.Should().Be(templatePersistence.TimePeriod);
        response?.TimeUnit.Should().Be(templatePersistence.TimeUnit);
        response?.PositionWidth.Should().Be(templatePersistence.PositionWidth);
        response?.PositionHeight.Should().Be(templatePersistence.PositionHeight);
        response?.SizeWidth.Should().Be(templatePersistence.SizeWidth);
        response?.SizeHeight.Should().Be(templatePersistence.SizeHeight);
    }

    [Fact]
    public async Task GetTemplateById_TemplateNotFound()
    {
        // Arrange
        await Authenticate();
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/Template/{IdGenerator.GenerateId()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Length.Should().Be(0);
    }
    
    [Fact]
    public async Task GetTemplatesByDashboardId_Successful()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "dashboard",
            OrganizationId = organization.Id.ToString()
        };
        await PopulateDatabase(new[] {dashboardPersistence});
        
        var templatePersistence = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = dashboardPersistence.Id.ToString(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };
        
        var templatePersistence1 = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = dashboardPersistence.Id.ToString(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };
        var templatePersistence2 = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };
        
        await PopulateDatabase(new[] {templatePersistence});
        await PopulateDatabase(new[] {templatePersistence1});
        await PopulateDatabase(new[] {templatePersistence2});

        
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/Template/all/{dashboardPersistence.Id.ToString()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<List<TemplateResponse>>(await httpResponseMessage.Content.ReadAsStringAsync());
        var templates = await GetAll<TemplatePersistenceModel>();
        var template1 = templates.Single(x => x.Id == ObjectId.Parse(templatePersistence.Id.ToString()));
        var template2 = templates.Single(x => x.Id == ObjectId.Parse(templatePersistence1.Id.ToString()));

        var responseTemplate1 = response?.Single(x => x.Id == templatePersistence.Id.ToString());
        responseTemplate1?.Id.Should().Be(template1.Id.ToString());
        responseTemplate1?.DashboardId.Should().Be(template1.DashboardId);
        responseTemplate1?.DatapointId.Should().Be(template1.DatapointId);
        responseTemplate1?.DisplayType.Should().Be(template1.DisplayType);
        responseTemplate1?.TimePeriod.Should().Be(template1.TimePeriod);
        responseTemplate1?.TimeUnit.Should().Be(template1.TimeUnit);
        responseTemplate1?.PositionWidth.Should().Be(template1.PositionWidth);
        responseTemplate1?.PositionHeight.Should().Be(template1.PositionHeight);
        responseTemplate1?.SizeWidth.Should().Be(template1.SizeWidth);
        responseTemplate1?.SizeHeight.Should().Be(template1.SizeHeight);
        
        var responseTemplate2 = response?.Single(x => x.Id == templatePersistence1.Id.ToString());
        responseTemplate2?.Id.Should().Be(template2.Id.ToString());
        responseTemplate2?.DashboardId.Should().Be(template2.DashboardId);
        responseTemplate2?.DatapointId.Should().Be(template2.DatapointId);
        responseTemplate2?.DisplayType.Should().Be(template2.DisplayType);
        responseTemplate2?.TimePeriod.Should().Be(template2.TimePeriod);
        responseTemplate2?.TimeUnit.Should().Be(template2.TimeUnit);
        responseTemplate2?.PositionWidth.Should().Be(template2.PositionWidth);
        responseTemplate2?.PositionHeight.Should().Be(template2.PositionHeight);
        responseTemplate2?.SizeWidth.Should().Be(template2.SizeWidth);
        responseTemplate2?.SizeHeight.Should().Be(template2.SizeHeight);

        response?.Count.Should().Be(2);
        templates.Length.Should().Be(3);
    }
    
    [Fact]
    public async Task GetTemplatesByDashboardId_DashboardNotFound()
    {
        // Arrange
        await Authenticate();
        var templatePersistence = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };
        
        await PopulateDatabase(new[] {templatePersistence});

        
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/Template/all/{IdGenerator.GenerateId()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Length.Should().Be(1);
    }
    
    [Fact]
    public async Task GetTemplatesByDashboardId_TemplatesNotFound()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "dashboard",
            OrganizationId = organization.Id.ToString()
        };
        await PopulateDatabase(new[] {dashboardPersistence});

        
        // Act
        var httpResponseMessage = await TestClient.GetAsync(new Uri($"/api/v1/Template/all/{dashboardPersistence.Id.ToString()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Should().BeEmpty();
    }
    
    [Fact]
    public async Task RemoveTemplate_Successful()
    {
        // Arrange
        await Authenticate();
        
        var templatePersistence = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        await PopulateDatabase(new[] {templatePersistence});

        // Act
        var httpResponseMessage = await TestClient.DeleteAsync(new Uri($"api/v1/Template/{templatePersistence.Id.ToString()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var templates = await GetAll<TemplatePersistenceModel>();

        templates.Should().BeEmpty();
    }
    
    [Fact]
    public async Task RemoveTemplate_TemplateNotFound()
    {
        // Arrange
        await Authenticate();
        
        var templatePersistence = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        await PopulateDatabase(new[] {templatePersistence});

        // Act
        var httpResponseMessage = await TestClient.DeleteAsync(new Uri($"api/v1/Template/{IdGenerator.GenerateId()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var templates = await GetAll<TemplatePersistenceModel>();

        templates.Length.Should().Be(1);
    }
}