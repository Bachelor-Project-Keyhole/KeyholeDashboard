using System.Security.Authentication;
using Domain.Datapoint;
using Domain.RepositoryInterfaces;
using EphemeralMongo;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Repository;
using Repository.Datapoint;
using Repository.Organization;

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

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IDataPointDomainService, DataPointDomainService>();
        services.AddScoped<IDatapointRepository, DatapointRepository>();
        services.AddScoped<IDatapointRepository, DatapointRepository>();
    }

    protected async Task PopulateDatabase<TDocument>(TDocument[] documents) where TDocument : IDocument
    {
        var collection = _database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        await collection.InsertManyAsync(documents);
    }

    protected async Task<TDocument[]> GetAll<TDocument>() where TDocument : IDocument
    {
        var collection = _database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        return collection.AsQueryable().ToArray();
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


