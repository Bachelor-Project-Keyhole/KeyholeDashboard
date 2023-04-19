using System.Security.Authentication;
using MongoDB.Driver;

namespace Repository;

public class DataAccess : IDataAccess
{
    private const string ConnectionString = @"mongodb://keyhole-dashboard-db:1diPD0VVkSVVqfIuWVHGe703gNSuTTI21Ubeg39f3Vodm1Is1AcrXaH7GmEAH7UJGtKv8SZXaDbYACDbJ3ynZA==@keyhole-dashboard-db.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@keyhole-dashboard-db@";
    private const string DatabaseName = "keyhole-dashboard-db";
    
    public IMongoCollection<T> ConnectToMongo<T>(string collection)
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
        settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
        var mongoClient = new MongoClient(settings);
        var db = mongoClient.GetDatabase(DatabaseName);
        return db.GetCollection<T>(collection);
    }
}