namespace Repository.Organization.OrganizationWriteModel;

public interface IOrganizationWriteModel
{
    Task Insert(OrganizationPersistenceModel organization);
}