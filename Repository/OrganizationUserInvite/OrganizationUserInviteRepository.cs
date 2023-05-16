using AutoMapper;
using Domain.Organization;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Repository.OrganizationUserInvite;

public class OrganizationUserInviteRepository :  MongoRepository<OrganizationUserInvitePersistence>, IOrganizationUserInviteRepository
{
    private readonly IMapper _mapper;
    
    public OrganizationUserInviteRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task InsertInviteUser(Domain.Organization.OrganizationUserInvites insert)
    {
        var invitesPersistence = _mapper.Map<OrganizationUserInvitePersistence>(insert);
        await InsertOneAsync(invitesPersistence);
    }

    public async Task UpdateUserInvite(OrganizationUserInvites insert)
    {
        var invite = _mapper.Map<OrganizationUserInvitePersistence>(insert);
        await ReplaceOneAsync(invite);
    }

    public async Task<Domain.Organization.OrganizationUserInvites?> GetByToken(string token)
    {
        // Create TTL functionality in either here or mongo Atlas
        var invitation = await Collection
            .Find(x => x.Token == token).SingleOrDefaultAsync();
        return _mapper.Map<Domain.Organization.OrganizationUserInvites>(invitation);
    }
}