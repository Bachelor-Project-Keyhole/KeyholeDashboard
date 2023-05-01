using EphemeralMongo;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Repository;

namespace WebApi.Tests;

public class IntegrationTest : IDisposable
{
    protected readonly HttpClient TestClient;
    private readonly IMongoRunner _runner;

    protected IntegrationTest()
    {
        var options = new MongoRunnerOptions
        {
            KillMongoProcessesWhenCurrentProcessExits = true // Default: false
        };
        _runner = MongoRunner.Run(options);

        var appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DatabaseOptions>();
                services.Configure<DatabaseOptions>(options =>
                {
                    options.MongoDbConnectionString = _runner.ConnectionString;
                    options.MongoDbDatabaseName = "TestDatabase";
                });
            });
        });
        
        TestClient = appFactory.CreateClient();
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}


