using System.Net;
using System.Text;
using Contracts.v1.Dashboard;
using Domain;
using Domain.Datapoint;
using Domain.Template;
using FluentAssertions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Repository.Dashboard;
using Repository.Datapoint;
using Repository.Template;

namespace WebApi.Tests.IntegrationTests.DashboardController;

public class DashboardControllerTests : IntegrationTest
{
    [Fact]
    public async Task CreateDashboard_Successful()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var request = new CreateDashboardRequest
        {
            OrganizationId = organization.Id.ToString(),
            DashboardName = "OrgName"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Dashboard", UriKind.Relative), stringContent);

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<DashboardResponse>(await httpResponseMessage.Content.ReadAsStringAsync());

        var dashboards = await GetAll<DashboardPersistenceModel>();
        var dashboard = dashboards.Single(x => x.Id.ToString() == response?.Id);
        dashboard.Name.Should().Be(request.DashboardName);
        dashboard.OrganizationId.Should().Be(organization.Id.ToString());
        dashboards.Length.Should().Be(1);
    }

    [Fact]
    public async Task CreateDashboard_OrganizationNotFound()
    {
        // Arrange
        await Authenticate();

        var request = new CreateDashboardRequest
        {
            OrganizationId = IdGenerator.GenerateId(),
            DashboardName = "OrgName"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PostAsync(new Uri("/api/v1/Dashboard", UriKind.Relative), stringContent);

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dashboards = await GetAll<DashboardPersistenceModel>();
        dashboards.Length.Should().Be(0);
    }

    [Fact]
    public async Task UpdateDashboard_Successful()
    {
        // Arrange
        await Authenticate();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };

        await PopulateDatabase(new[] { dashboardPersistence });

        var request = new UpdateDashboardRequest
        {
            DashboardId = dashboardPersistence.Id.ToString(),
            DashboardName = "OrgUpdated"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PutAsync(new Uri("/api/v1/Dashboard", UriKind.Relative), stringContent);

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<DashboardResponse>(await httpResponseMessage.Content.ReadAsStringAsync());

        var dashboards = await GetAll<DashboardPersistenceModel>();
        var dashboard = dashboards.Single(x => x.Id.ToString() == response?.Id);
        dashboard.Name.Should().Be(request.DashboardName);
        dashboards.Length.Should().Be(1);
    }

    [Fact]
    public async Task UpdateDashboard_DashboardNotFound()
    {
        // Arrange
        await Authenticate();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };

        await PopulateDatabase(new[] { dashboardPersistence });

        var request = new UpdateDashboardRequest
        {
            DashboardId = IdGenerator.GenerateId(),
            DashboardName = "OrgUpdated"
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var httpResponseMessage =
            await TestClient.PutAsync(new Uri("/api/v1/Dashboard", UriKind.Relative), stringContent);

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dashboards = await GetAll<DashboardPersistenceModel>();
        dashboards.Length.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardById_Successful()
    {
        // Arrange
        await Authenticate();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };
        await PopulateDatabase(new[] { dashboardPersistence });

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Dashboard/{dashboardPersistence.Id.ToString()}",
                UriKind.Relative));

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<DashboardResponse>(await httpResponseMessage.Content.ReadAsStringAsync());

        var dashboards = await GetAll<DashboardPersistenceModel>();
        var dashboard = dashboards.Single(x => x.Id.ToString() == response?.Id);
        response?.Name.Should().Be(dashboardPersistence.Name);
        response?.OrganizationId.Should().Be(dashboard.OrganizationId);
    }

    [Fact]
    public async Task GetDashboardById_DashboardNotFound()
    {
        // Arrange
        await Authenticate();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };
        await PopulateDatabase(new[] { dashboardPersistence });

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Dashboard/{IdGenerator.GenerateId()}", UriKind.Relative));

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dashboards = await GetAll<DashboardPersistenceModel>();
        dashboards.Length.Should().Be(1);
    }


    [Fact]
    public async Task GetDashboardsByOrganizationId_Successful()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence1 = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = organization.Id.ToString()
        };
        var dashboardPersistence2 = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = organization.Id.ToString()
        };
        var dashboardPersistence3 = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };
        await PopulateDatabase(new[] { dashboardPersistence1, dashboardPersistence2, dashboardPersistence3 });

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Dashboard/all/{organization.Id.ToString()}", UriKind.Relative));

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<List<DashboardResponse>>(
                await httpResponseMessage.Content.ReadAsStringAsync());

        var dashboards = await GetAll<DashboardPersistenceModel>();
        var response1 = response?.Single(x => x.Id == dashboardPersistence1.Id.ToString());
        response1?.Name.Should().Be(dashboardPersistence1.Name);
        response1?.OrganizationId.Should().Be(organization.Id.ToString());

        var response2 = response?.Single(x => x.Id == dashboardPersistence2.Id.ToString());
        response2?.Name.Should().Be(dashboardPersistence2.Name);
        response2?.OrganizationId.Should().Be(organization.Id.ToString());

        response?.Count.Should().Be(2);
        dashboards.Length.Should().Be(3);
    }
    
    [Fact]
    public async Task GetDashboardsByOrganizationId_ReturnsEmptyArray()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();
        var dashboardPersistence3 = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };
        await PopulateDatabase(new[] { dashboardPersistence3 });

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Dashboard/all/{organization.Id.ToString()}", UriKind.Relative));

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<List<DashboardResponse>>(
                await httpResponseMessage.Content.ReadAsStringAsync());
        response.Should().NotBeNull();
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardsByOrganizationId_OrganizationDoesNotExist()
    {
        // Arrange
        await Authenticate();

        var dashboardPersistence1 = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };

        await PopulateDatabase(new[] { dashboardPersistence1 });

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Dashboard/all/{dashboardPersistence1.OrganizationId}",
                UriKind.Relative));

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dashboards = await GetAll<DashboardPersistenceModel>();
        dashboards.Length.Should().Be(1);
    }


    [Fact]
    public async Task GetDashboardsByOrganizationId_DashboardsNotFound()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence1 = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "OrgName",
            OrganizationId = organization.Id.ToString()
        };
        await PopulateDatabase(new[] { dashboardPersistence1 });


        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Dashboard/all/{IdGenerator.GenerateId()}", UriKind.Relative));

        // Assert 
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dashboards = await GetAll<DashboardPersistenceModel>();
        dashboards.Length.Should().Be(1);
    }

    [Fact]
    public async Task RemoveDashboard_Successful_WhenTemplatesPresent()
    {
        // Arrange
        await Authenticate();

        var dashboardId = ObjectId.Parse(IdGenerator.GenerateId());

        var dashboardPersistence1 = new DashboardPersistenceModel
        {
            Id = dashboardId,
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };
        await PopulateDatabase(new[] { dashboardPersistence1 });

        var templatePersistence1 = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = dashboardId.ToString(),
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

        await PopulateDatabase(new[] { templatePersistence1, templatePersistence2 });

        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"/api/v1/Dashboard/{dashboardPersistence1.Id.ToString()}",
                UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboards = await GetAll<DashboardPersistenceModel>();
        var templates = await GetAll<TemplatePersistenceModel>();

        dashboards.Length.Should().Be(0);
        templates.Length.Should().Be(1);
        var template = templates.Single(x => x.Id == templatePersistence2.Id);
        template.DashboardId.Should().NotBe(dashboardId.ToString());
    }

    [Fact]
    public async Task RemoveDashboard_Successful_WhenTemplateNotPresent()
    {
        // Arrange
        await Authenticate();

        var dashboardId = ObjectId.Parse(IdGenerator.GenerateId());

        var dashboardPersistence1 = new DashboardPersistenceModel
        {
            Id = dashboardId,
            Name = "OrgName",
            OrganizationId = IdGenerator.GenerateId()
        };
        await PopulateDatabase(new[] { dashboardPersistence1 });


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

        await PopulateDatabase(new[] { templatePersistence2 });

        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"/api/v1/Dashboard/{dashboardPersistence1.Id.ToString()}",
                UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboards = await GetAll<DashboardPersistenceModel>();
        var templates = await GetAll<TemplatePersistenceModel>();

        dashboards.Length.Should().Be(0);
        templates.Length.Should().Be(1);
        var template = templates.Single(x => x.Id == templatePersistence2.Id);
        template.DashboardId.Should().NotBe(dashboardId.ToString());
    }

    [Fact]
    public async Task RemoveDashboard_DashboardDoesNotExist_WhenTemplatesPresent()
    {
        // Arrange
        await Authenticate();

        var dashboardId = ObjectId.Parse(IdGenerator.GenerateId());

        var templatePersistence2 = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = dashboardId.ToString(),
            DatapointId = IdGenerator.GenerateId(),
            DisplayType = DisplayType.Numeric,
            TimePeriod = 30,
            TimeUnit = TimeUnit.Day,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        await PopulateDatabase(new[] { templatePersistence2 });

        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"/api/v1/Dashboard/{dashboardId}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dashboards = await GetAll<DashboardPersistenceModel>();
        dashboards.Length.Should().Be(0);
        var templates = await GetAll<TemplatePersistenceModel>();
        templates.Length.Should().Be(1);
    }

    [Fact]
    public async Task RemoveDashboard_DashboardDoesNotExist_WhenTemplateNotPresent()
    {
        // Arrange
        await Authenticate();

        var dashboardId = ObjectId.Parse(IdGenerator.GenerateId());

        // Act
        var httpResponseMessage =
            await TestClient.DeleteAsync(new Uri($"/api/v1/Dashboard/{dashboardId}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var dashboards = await GetAll<DashboardPersistenceModel>();
        dashboards.Length.Should().Be(0);
    }

    [Fact]
    public async Task LoadDashboard_Successful()
    {
        // Arrange
        await Authenticate();
        var organization = await SetupOrganization();

        var dashboardPersistence = new DashboardPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Name = "Dashboard name",
            OrganizationId = organization.Id.ToString()
        };
        await PopulateDatabase(new[] { dashboardPersistence });

        var datapoint = new DataPointEntity(organization.Id.ToString(), "Testy", "Testy", true, true, 502)
        {
            Formula = new Formula
            {
                Factor = 2,
                Operation = MathOperation.Add
            }
        };

        await PopulateDatabase(new[] { datapoint });

        var dataEntry1 = new DataPointEntryEntity(organization.Id.ToString(), datapoint.DataPointKey, 50,
            DateTime.UtcNow.AddDays(-10));
        var dataEntry2 = new DataPointEntryEntity(organization.Id.ToString(), datapoint.DataPointKey, 25,
            DateTime.UtcNow.AddDays(-8));
        var dataEntry3 = new DataPointEntryEntity(organization.Id.ToString(), datapoint.DataPointKey, -100,
            DateTime.UtcNow.AddDays(-6));
        var dataEntry4 = new DataPointEntryEntity(organization.Id.ToString(), datapoint.DataPointKey, -15,
            DateTime.UtcNow.AddDays(-4));
        var dataEntry5 = new DataPointEntryEntity(organization.Id.ToString(), datapoint.DataPointKey, 500,
            DateTime.UtcNow.AddDays(-1));

        await PopulateDatabase(new[] { dataEntry1, dataEntry2, dataEntry3, dataEntry4, dataEntry5 });

        var templatePersistence1 = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = dashboardPersistence.Id.ToString(),
            DatapointId = datapoint.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimeUnit = TimeUnit.Week,
            TimePeriod = 1,
            PositionHeight = 1,
            PositionWidth = 1,
            SizeHeight = 1,
            SizeWidth = 1
        };

        var templatePersistence2 = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = dashboardPersistence.Id.ToString(),
            DatapointId = datapoint.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimeUnit = TimeUnit.Day,
            TimePeriod = 5,
            PositionHeight = 1,
            PositionWidth = 2,
            SizeHeight = 1,
            SizeWidth = 1
        };

        var templatePersistence3 = new TemplatePersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            DashboardId = IdGenerator.GenerateId(),
            DatapointId = datapoint.Id.ToString(),
            DisplayType = DisplayType.Numeric,
            TimeUnit = TimeUnit.Day,
            TimePeriod = 5,
            PositionHeight = 1,
            PositionWidth = 2,
            SizeHeight = 1,
            SizeWidth = 1
        };

        await PopulateDatabase(new[] { templatePersistence1, templatePersistence2, templatePersistence3 });

        // Act
        var httpResponseMessage =
            await TestClient.GetAsync(new Uri($"/api/v1/Dashboard/load/{dashboardPersistence.Id.ToString()}",
                UriKind.Relative));

        // Arrange
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var response =
            JsonConvert.DeserializeObject<DashboardAndElementsResponse>(await httpResponseMessage.Content
                .ReadAsStringAsync());

        response?.DashboardId.Should().Be(dashboardPersistence.Id.ToString());
        response?.DashboardName.Should().Be(dashboardPersistence.Name);

        var responsePlaceholder1 =
            response?.Placeholders.Single(x => x.TemplateId == templatePersistence1.Id.ToString());
        responsePlaceholder1?.LatestValue.Should().Be(datapoint.LatestValue);
        responsePlaceholder1?.Comparison.Should().Be(datapoint.IsComparisonAbsolute);
        responsePlaceholder1?.Change.Should().Be(475);
        responsePlaceholder1?.Values.Should().Contain(x => Math.Abs(x.Value - dataEntry3.Value) < 0.001);
        responsePlaceholder1?.Values.Should().Contain(x => Math.Abs(x.Value - dataEntry4.Value) < 0.001);
        responsePlaceholder1?.Values.Should().Contain(x => Math.Abs(x.Value - dataEntry5.Value) < 0.001);
        responsePlaceholder1?.Values.Should().NotContain(x => Math.Abs(x.Value - dataEntry2.Value) < 0.001);
        responsePlaceholder1?.Values.Should().NotContain(x => Math.Abs(x.Value - dataEntry1.Value) < 0.001);
        responsePlaceholder1?.SizeHeight.Should().Be(templatePersistence1.SizeHeight);
        responsePlaceholder1?.SizeWidth.Should().Be(templatePersistence1.SizeWidth);
        responsePlaceholder1?.PositionHeight.Should().Be(templatePersistence1.PositionHeight);
        responsePlaceholder1?.PositionWidth.Should().Be(templatePersistence1.PositionWidth);
        responsePlaceholder1?.DisplayName.Should().Be(datapoint.DisplayName);

        var responsePlaceholder2 =
            response?.Placeholders.Single(x => x.TemplateId == templatePersistence2.Id.ToString());
        responsePlaceholder2?.LatestValue.Should().Be(datapoint.LatestValue);
        responsePlaceholder2?.Comparison.Should().Be(datapoint.IsComparisonAbsolute);
        responsePlaceholder2?.Change.Should().Be(600);
        responsePlaceholder2?.Values.Should().Contain(x => Math.Abs(x.Value - dataEntry4.Value) < 0.001);
        responsePlaceholder2?.Values.Should().Contain(x => Math.Abs(x.Value - dataEntry5.Value) < 0.001);
        responsePlaceholder2?.Values.Should().NotContain(x => Math.Abs(x.Value - dataEntry3.Value) < 0.001);
        responsePlaceholder2?.Values.Should().NotContain(x => Math.Abs(x.Value - dataEntry2.Value) < 0.001);
        responsePlaceholder2?.Values.Should().NotContain(x => Math.Abs(x.Value - dataEntry1.Value) < 0.001);
        responsePlaceholder2?.SizeHeight.Should().Be(templatePersistence2.SizeHeight);
        responsePlaceholder2?.SizeWidth.Should().Be(templatePersistence2.SizeWidth);
        responsePlaceholder2?.PositionHeight.Should().Be(templatePersistence2.PositionHeight);
        responsePlaceholder2?.PositionWidth.Should().Be(templatePersistence2.PositionWidth);
        responsePlaceholder2?.DisplayName.Should().Be(datapoint.DisplayName);

        response?.Placeholders.Count.Should().Be(2);
    }
}