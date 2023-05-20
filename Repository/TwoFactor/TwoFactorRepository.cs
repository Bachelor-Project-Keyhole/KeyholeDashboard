using AutoMapper;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Repository.TwoFactor;

public class TwoFactorRepository : MongoRepository<TwoFactorPersistence>, ITwoFactorRepository
{

    private readonly IMapper _mapper;

    public TwoFactorRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }
    public async Task<Domain.TwoFactor.TwoFactor?> GetByIdentifier(string email)
    {
        var twoFactorPersistence = await FindOneAsync(x => x.Identifier == email);
        if (twoFactorPersistence == null)
            return null;

        var twoFactor = _mapper.Map<Domain.TwoFactor.TwoFactor>(twoFactorPersistence);
        return twoFactor;

    }

    public async Task Delete(string id)
    {
        await DeleteOneAsync(x => x.Id == ObjectId.Parse(id));
    }

    public async Task Insert(Domain.TwoFactor.TwoFactor twoFactor)
    {
        var twoFactorPersistence = _mapper.Map<TwoFactorPersistence>(twoFactor);
        await InsertOneAsync(twoFactorPersistence);
    }
}