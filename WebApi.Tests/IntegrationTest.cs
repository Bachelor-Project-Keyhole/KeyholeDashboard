using System.Security.Authentication;
using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Service;
using Application.User.UserService;
using Domain;
using Domain.Datapoint;
using Domain.RepositoryInterfaces;
using EphemeralMongo;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository;
using Repository.Datapoint;
using Repository.Organization;
using Repository.TwoFactor;
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

    protected async Task<string> SetupOrganization()
    {
        var organizationId = IdGenerator.GenerateId();
        var organizationEntity = new OrganizationEntity
        {
            Id = new ObjectId(organizationId),
            OrganizationName = "Organization"
        };
        await PopulateDatabase(new []{organizationEntity});
        return organizationId;
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
        return ((BsonCollectionAttribute) documentType.GetCustomAttributes(
                typeof(BsonCollectionAttribute),
                true)
            .FirstOrDefault()!).CollectionName ?? throw new InvalidOperationException();
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}


