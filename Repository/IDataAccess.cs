using MongoDB.Driver;

namespace Repository;

public interface IDataAccess
{
    IMongoCollection<T> ConnectToMongo<T>(string collection);
}