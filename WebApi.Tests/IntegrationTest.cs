using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Helper;
using Application.JWT.Model;
using Application.JWT.Service;
using Application.User.UserService;
using Domain;
using Domain.Datapoint;
using Domain.RepositoryInterfaces;
using Domain.User;
using EphemeralMongo;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Repository;
using Repository.Datapoint;
using Repository.Organization;
using Repository.TwoFactor;
using Repository.User.UserPersistence;
using Repository.User.UserRepository;

namespace WebApi.Tests;

public class IntegrationTest : IDisposable
{
    protected readonly HttpClient TestClient;
    private readonly IMongoRunner _runner;
    private const string TestDatabaseName = "TestDatabase";
    private readonly IMongoDatabase _database;

    protected IntegrationTest()
    {
        var options = new MongoRunnerOptions
        {
            KillMongoProcessesWhenCurrentProcessExits = true // Default: false
        };
        _runner = MongoRunner.Run(options);

        var settings = MongoClientSettings.FromUrl(new MongoUrl(_runner.ConnectionString));
        settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
        var mongoClient = new MongoClient(settings);
        _database = mongoClient.GetDatabase(TestDatabaseName);

        var appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(ConfigureServices);
        });

        TestClient = appFactory.CreateClient();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<DatabaseOptions>();
        services.Configure<DatabaseOptions>(databaseOptions =>
        {
            databaseOptions.MongoDbConnectionString = _runner.ConnectionString;
            databaseOptions.MongoDbDatabaseName = TestDatabaseName;
        });

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IUserAuthenticationService, UserAuthenticationService>();
        services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ITwoFactorRepository, TwoFactorRepository>();
        services.AddTransient<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IDataPointDomainService, DataPointDomainService>();
        services.AddScoped<IDataPointRepository, DataPointRepository>();
        services.AddScoped<IDataPointRepository, DataPointRepository>();
    }

    protected async Task<OrganizationPersistenceModel> SetupOrganization()
    {
        var organization = new OrganizationPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            OrganizationName = "wow",
            OrganizationOwnerId = "aa",
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { organization });

        return organization;
    }

    protected async Task PopulateDatabase<TDocument>(TDocument[] documents) where TDocument : IDocument
    {
        var collection = _database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        await collection.InsertManyAsync(documents);
    }

    protected async Task<TDocument[]> GetAll<TDocument>() where TDocument : IDocument
    {
        var collection = _database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        var result = await collection.AsQueryable().ToListAsync();
        return result.ToArray();
    }

    private string GetCollectionName(Type documentType)
    {
        return ((BsonCollectionAttribute)documentType.GetCustomAttributes(
                typeof(BsonCollectionAttribute),
                true)
            .FirstOrDefault()!).CollectionName ?? throw new InvalidOperationException();
    }

    protected async Task Authenticate()
    {
        var password = "orange1234";
        var userPersistence1 = new UserPersistenceModel
        {
            Id = ObjectId.Parse(IdGenerator.GenerateId()),
            Email = "auth@auth.com",
            OwnedOrganizationId = "",
            MemberOfOrganizationId = "646791352d33a03d8d495c2e",
            FullName = "Yo lama1",
            PasswordHash = PasswordHelper.GetHashedPassword(password), // Has to be at least 8 chars
            AccessLevels = new List<UserAccessLevel>
                { UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin },
            RefreshTokens = new List<PersistenceRefreshToken>(),
            ModifiedDate = DateTime.UtcNow,
            RegistrationDate = DateTime.UtcNow
        };

        await PopulateDatabase(new[] { userPersistence1 });

        var auth = new AuthenticateRequest
        {
            Email = userPersistence1.Email,
            Password = password
        };


        var stringContentAuth = new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json");

        var httpResponseMessageAuth =
            await TestClient.PostAsync(new Uri($"api/v1/Authentication/login", UriKind.Relative), stringContentAuth);
        var responseAuth =
            JsonConvert.DeserializeObject<AuthenticationResponse>(await httpResponseMessageAuth.Content
                .ReadAsStringAsync());


        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", responseAuth?.Token);
    }


    public void Dispose()
    {
        _runner.Dispose();
    }
}