using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Repository;

public interface IMongoRepository<TDocument> where TDocument : IDocument
{
    [UsedImplicitly]
    IQueryable<TDocument> AsQueryable();
    
    [UsedImplicitly]
    Task<IEnumerable<TDocument>> FilterByAsync(Expression<Func<TDocument, bool>> filterExpression);
    
    [UsedImplicitly]
    IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression);
    
    [UsedImplicitly]
    Task<TDocument?> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression);
    
    [UsedImplicitly]
    Task<TDocument?> FindByIdAsync(string id);
    
    [UsedImplicitly]
    Task InsertOneAsync(TDocument document);
    
    [UsedImplicitly]
    Task InsertManyAsync(ICollection<TDocument> documents);

    [UsedImplicitly]
    Task ReplaceOneAsync(TDocument document);

    [UsedImplicitly]
    Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression);

    [UsedImplicitly]
    Task DeleteByIdAsync(string id);

    [UsedImplicitly]
    Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression);
}