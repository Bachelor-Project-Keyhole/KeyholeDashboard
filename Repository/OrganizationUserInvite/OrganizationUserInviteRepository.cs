using AutoMapper;
using Domain.Organization;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;

namespace Repository.OrganizationUserInvite;

public class OrganizationUserInviteRepository :  MongoRepository<OrganizationUserInvitePersistence>, IOrganizationUserInviteRepository
{
    private readonly IMapper _mapper;
    
    public OrganizationUserInviteRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task InsertInviteUser(OrganizationUserInvites insert)
    {
        var invitesPersistence = _mapper.Map<OrganizationUserInvitePersistence>(insert);
        await InsertOneAsync(invitesPersistence);
    }
}