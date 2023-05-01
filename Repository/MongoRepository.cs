using System.Linq.Expressions;
using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository;

public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : IDocument
{
    private readonly IMongoCollection<TDocument> _collection;

    public MongoRepository(IOptions<DatabaseOptions> dataBaseOptions)
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(dataBaseOptions.Value.MongoDbConnectionString));
        settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
        var mongoClient = new MongoClient(settings);
        var db = mongoClient.GetDatabase(dataBaseOptions.Value.MongoDbDatabaseName);
        _collection = db.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
    }

    private string GetCollectionName(Type documentType)
    {
        return ((BsonCollectionAttribute) documentType.GetCustomAttributes(
                typeof(BsonCollectionAttribute),
                true)
            .FirstOrDefault()!).CollectionName ?? throw new InvalidOperationException();
    }

    public virtual IQueryable<TDocument> AsQueryable()
    {
        return _collection.AsQueryable();
    }

    public virtual IEnumerable<TDocument> FilterBy(
        Expression<Func<TDocument, bool>> filterExpression)
    {
        return _collection.Find(filterExpression).ToEnumerable();
    }

    public virtual IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression)
    {
        return _collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
    }
    
    public virtual async Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        return await _collection.Find(filterExpression).FirstOrDefaultAsync();
    }
    
    public virtual async Task<TDocument> FindByIdAsync(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
        return await _collection.Find(filter).SingleOrDefaultAsync();
    }
    
    public virtual async Task InsertOneAsync(TDocument document)
    {
        await _collection.InsertOneAsync(document);
    }
    
    public virtual async Task InsertManyAsync(ICollection<TDocument> documents)
    {
        await _collection.InsertManyAsync(documents);
    }
    
    public virtual async Task ReplaceOneAsync(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await _collection.FindOneAndReplaceAsync(filter, document);
    }
    
    public async Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    { 
        await _collection.FindOneAndDeleteAsync(filterExpression);
    }
    
    public async Task DeleteByIdAsync(string id)
    { 
        var objectId = new ObjectId(id); 
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId); 
        await _collection.FindOneAndDeleteAsync(filter);
    }
    
    public async Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        await _collection.DeleteManyAsync(filterExpression);
    }
}