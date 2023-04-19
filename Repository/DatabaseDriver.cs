using System.Security.Authentication;
using MongoDB.Driver;

namespace Repository;

public class DatabaseDriver
{
    public MongoClient Client { get; private set; }
    
    public DatabaseDriver()
    {
        string connectionString = 
            @"mongodb://keyhole-dashboard-db:1diPD0VVkSVVqfIuWVHGe703gNSuTTI21Ubeg39f3Vodm1Is1AcrXaH7GmEAH7UJGtKv8SZXaDbYACDbJ3ynZA==@keyhole-dashboard-db.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@keyhole-dashboard-db@";
        var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
        settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
        Client = new MongoClient(settings);
    }
}