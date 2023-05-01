using Domain;
using EphemeralMongo;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Repository;

namespace WebApi.Tests.RepositoryTests;

public class MongoRepositoryTests : IDisposable
{
    private IMongoRunner _runner;
    
    private IOptions<DatabaseOptions> GetInMemoryDatabaseOptions()
    {
        var options = new MongoRunnerOptions
        {
            KillMongoProcessesWhenCurrentProcessExits = true
        };
        _runner = MongoRunner.Run(options);

        return Options.Create(new DatabaseOptions()
        {
            MongoDbConnectionString = _runner.ConnectionString,
            MongoDbDatabaseName = "TestDatabase"
        });
    }
    
    [Fact]
    public async Task InsertOneAsync_ShouldInsertDocument()
    {
        //Arrange
        var databaseOptions = GetInMemoryDatabaseOptions();
        var repository = new MongoRepository<TestEntity>(databaseOptions);
        var testEntity = new TestEntity("Value", 42, 23.32);

        //Act
        await repository.InsertOneAsync(testEntity);

        //Assert
        repository.AsQueryable().ToArray().Single().Should().BeEquivalentTo(testEntity);
    }
    
    [Fact]
    public async Task InsertManyAsync_ShouldInsertAllDocuments()
    {
        //Arrange
        var databaseOptions = GetInMemoryDatabaseOptions();
        var repository = new MongoRepository<TestEntity>(databaseOptions);
        var testEntities = new List<TestEntity>
        {
            new("Value", 42, 23.32),
            new("Value2", 24, 32.23)
        };

        //Act
        await repository.InsertManyAsync(testEntities);

        //Assert
        repository.AsQueryable().ToArray().Should().HaveCount(2).And.BeEquivalentTo(testEntities);
    }
    
    [Fact]
    public async Task DeleteByIdAsync_ShouldDeleteDocumentWithMatchingId()
    {
        //Arrange
        var databaseOptions = GetInMemoryDatabaseOptions();
        var repository = new MongoRepository<TestEntity>(databaseOptions);
        var entityToDelete = new TestEntity("Value", 12, 45.23);
        var documentId = IdGenerator.GenerateId();
        entityToDelete.Id = new ObjectId(documentId);
        
        var testEntities = new List<TestEntity>
        {
            new("Value1", 42, 23.32),
            new("Value2", 24, 32.23),
            entityToDelete
        };
        await repository.InsertManyAsync(testEntities);

        //Act
        await repository.DeleteByIdAsync(documentId);

        //Assert
        repository.AsQueryable().ToArray().Should().HaveCount(2).And.NotContain(entityToDelete);
    }
    
    [Fact]
    public async Task DeleteManyAsync_ShouldDeleteAllDocumentsThatMatchTheExpression()
    {
        //Arrange
        var databaseOptions = GetInMemoryDatabaseOptions();
        var repository = new MongoRepository<TestEntity>(databaseOptions);
        var testEntities = new List<TestEntity>
        {
            new("Value1", 42, 23.32),
            new("Value2", 90, 32.23),
            new("Value3", 100, 32.23),
        };
        await repository.InsertManyAsync(testEntities);

        //Act
        await repository.DeleteManyAsync(x => x.IntValue > 50);

        //Assert
        repository.AsQueryable().ToArray().Should().HaveCount(1);
    }

    [Fact]
    public async Task ReplaceOneAsync_ShouldReplaceDocumentWithMatchingId()
    {
        //Arrange
        var databaseOptions = GetInMemoryDatabaseOptions();
        var repository = new MongoRepository<TestEntity>(databaseOptions);
        
        var entityToReplace = new TestEntity("Value", 12, 45.23);
        var documentId = IdGenerator.GenerateId();
        entityToReplace.Id = new ObjectId(documentId);
        var testEntities = new List<TestEntity>
        {
            new("Value1", 42, 23.32),
            new("Value2", 90, 32.23),
            entityToReplace
        };
        await repository.InsertManyAsync(testEntities);

        entityToReplace.StringValue = "Updated Value";
        entityToReplace.IntValue = 99;

        //Act
        await repository.ReplaceOneAsync(entityToReplace);

        //Assert
        repository.AsQueryable().ToArray().Should().HaveCount(3);
        var result = await repository.FindByIdAsync(entityToReplace.Id.ToString());
        result.Should().BeEquivalentTo(entityToReplace);
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}