using MongoDB.Driver;

namespace Repository.Organization.OrganizationWriteModel;

public class OrganizationWriteModel : IOrganizationWriteModel
{
    private readonly IMongoDatabase _database;

    public OrganizationWriteModel(IMongoDatabase database)
    {
        _database = database;
    }
    
    public async Task Insert(OrganizationPersistenceModel organization)
    {
        await _database.GetCollection<OrganizationPersistenceModel>(nameof(Domain.DomainEntities.Organization))
            .InsertOneAsync(organization);
    }
}