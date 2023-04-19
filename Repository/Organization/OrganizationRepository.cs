using AutoMapper;
using Domain.RepositoryInterfaces;
using Repository.Organization.OrganizationReadModel;
using Repository.Organization.OrganizationWriteModel;

namespace Repository.Organization;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly IMapper _mapper;
    private readonly IOrganizationReadModel _organizationReadModel;
    private readonly IOrganizationWriteModel _organizationWriteModel;

    public OrganizationRepository(
        IMapper mapper,
        IOrganizationReadModel organizationReadModel,
        IOrganizationWriteModel organizationWriteModel)
    {
        _mapper = mapper;
        _organizationReadModel = organizationReadModel;
        _organizationWriteModel = organizationWriteModel;
    }
    public async Task Insert(Domain.DomainEntities.Organization organization)
    {
        var persistenceOrganization = _mapper.Map<OrganizationPersistenceModel>(organization);
        await _organizationWriteModel.Insert(persistenceOrganization);
    }
}