using AutoMapper;
using Domain.Organization.OrganizationUserInvite;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository.OrganizationUserInvite;

public class OrganizationUserInviteRepository :  MongoRepository<OrganizationUserInvitePersistence>, IOrganizationUserInviteRepository
{
    private readonly IMapper _mapper;
    
    public OrganizationUserInviteRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task<List<OrganizationUserInvites>?> GetAllInvitesByOrganizationId(string organizationId)
    {
        var invitations = await FilterByAsync(x => x.OrganizationId == organizationId && x.OrganizationId == organizationId);
        return _mapper.Map<List<OrganizationUserInvites>>(invitations);
    }

    public async Task<OrganizationUserInvites?> GetInvitationById(string invitationId, string organizationId)
    {
        var invitation = await FindByIdAsync(invitationId);
        return _mapper.Map<OrganizationUserInvites>(invitation);
    }

    public async Task<OrganizationUserInvites?> GetInvitationByEmail(string email, string organizationId)
    {
        var invitation = await FindOneAsync(x => x.ReceiverEmail == email && x.OrganizationId == organizationId);
        return _mapper.Map<OrganizationUserInvites>(invitation);
    }

    public async Task InsertInviteUser(OrganizationUserInvites insert)
    {
        var invitesPersistence = _mapper.Map<OrganizationUserInvitePersistence>(insert);
        await InsertOneAsync(invitesPersistence);
    }

    public async Task UpdateUserInvite(OrganizationUserInvites insert)
    {
        var invite = _mapper.Map<OrganizationUserInvitePersistence>(insert);
        await ReplaceOneAsync(invite);
    }

    public async Task RemoveInvitationByToken(string token)
    {
        await DeleteOneAsync(x => x.Token == token);
    }

    public async Task<OrganizationUserInvites?> GetInvitationByToken(string token)
    {
        // Create TTL functionality in either here or mongo Atlas
        var invitation = await Collection
            .Find(x => x.Token == token).SingleOrDefaultAsync();
        return _mapper.Map<OrganizationUserInvites>(invitation);
    }

    public async Task RemoveInvitationById(string invitationId)
    {
        await DeleteOneAsync(x => x.Id == ObjectId.Parse(invitationId));
    }
}