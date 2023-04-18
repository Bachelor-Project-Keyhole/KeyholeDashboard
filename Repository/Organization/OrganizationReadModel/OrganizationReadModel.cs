using MongoDB.Driver;

namespace Repository.Organization.OrganizationReadModel;

public class OrganizationReadModel : IOrganizationReadModel
{
    private readonly IMongoDatabase _database;

    public OrganizationReadModel(IMongoDatabase database)
    {
        _database = database;
    }


}