using System.Linq.Expressions;
using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Repository;

public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : IDocument
{
    protected readonly IMongoCollection<TDocument> Collection;

    public MongoRepository(IOptions<DatabaseOptions> dataBaseOptions)
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(dataBaseOptions.Value.MongoDbConnectionString));
        settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
        var mongoClient = new MongoClient(settings);
        var db = mongoClient.GetDatabase(dataBaseOptions.Value.MongoDbDatabaseName);
        Collection = db.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
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
        return Collection.AsQueryable();
    }
    
    public virtual async Task<IEnumerable<TDocument>> FilterByAsync(Expression<Func<TDocument, bool>> predicate)
    {
        var filter =  Collection.AsQueryable().Where(predicate);
        return await filter.ToListAsync();
    }

    public virtual IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression)
    {
        return Collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
    }
    
    public virtual async Task<TDocument?> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        return await Collection.Find(filterExpression).FirstOrDefaultAsync();
    }
    
    public virtual async Task<TDocument?> FindByIdAsync(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
        return await Collection.Find(filter).SingleOrDefaultAsync();
    }
    
    public virtual async Task InsertOneAsync(TDocument document)
    {
        await Collection.InsertOneAsync(document);
    }
    
    public virtual async Task InsertManyAsync(ICollection<TDocument> documents)
    {
        await Collection.InsertManyAsync(documents);
    }
    
    public virtual async Task ReplaceOneAsync(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await Collection.FindOneAndReplaceAsync(filter, document);
    }
    
    public async Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    { 
        await Collection.FindOneAndDeleteAsync(filterExpression);
    }
    
    public async Task DeleteByIdAsync(string id)
    { 
        var objectId = new ObjectId(id); 
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId); 
        await Collection.FindOneAndDeleteAsync(filter);
    }
    
    public async Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        await Collection.DeleteManyAsync(filterExpression);
    }
}