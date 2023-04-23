using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Repository;

public interface IMongoRepository<TDocument> where TDocument : IDocument
{
    [UsedImplicitly]
    IQueryable<TDocument> AsQueryable();
    
    [UsedImplicitly]
    IEnumerable<TDocument> FilterBy(Expression<Func<TDocument, bool>> filterExpression);
    
    [UsedImplicitly]
    IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression);
    
    [UsedImplicitly]
    TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression);
    
    [UsedImplicitly]
    Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression);
    
    [UsedImplicitly]
    TDocument FindById(string id);
    
    [UsedImplicitly]
    Task<TDocument> FindByIdAsync(string id);
    
    [UsedImplicitly]
    void InsertOne(TDocument document);
    
    [UsedImplicitly]
    Task InsertOneAsync(TDocument document);
    
    [UsedImplicitly]
    void InsertMany(ICollection<TDocument> documents);
    
    [UsedImplicitly]
    Task InsertManyAsync(ICollection<TDocument> documents);
    
    [UsedImplicitly]
    void ReplaceOne(TDocument document);
    
    [UsedImplicitly]
    Task ReplaceOneAsync(TDocument document);
    
    [UsedImplicitly]
    void DeleteOne(Expression<Func<TDocument, bool>> filterExpression);
    
    [UsedImplicitly]
    Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression);
    
    [UsedImplicitly]
    void DeleteById(string id);
    
    [UsedImplicitly]
    Task DeleteByIdAsync(string id);
    
    [UsedImplicitly]
    void DeleteMany(Expression<Func<TDocument, bool>> filterExpression);
    
    [UsedImplicitly]
    Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression);
}