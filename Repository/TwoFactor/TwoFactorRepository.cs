using AutoMapper;
using Domain.RepositoryInterfaces;
using MongoDB.Bson;
using Repository.TwoFactor.TwoFactorReadModel;
using Repository.TwoFactor.TwoFactorWriteModel;

namespace Repository.TwoFactor;

public class TwoFactorRepository : ITwoFactorRepository
{
    private readonly ITwoFactorReadModel _twoFactorReadModel;
    private readonly ITwoFactorWriteModel _twoFactorWriteModel;
    private readonly IMapper _mapper;

    public TwoFactorRepository(
        ITwoFactorReadModel twoFactorReadModel,
        ITwoFactorWriteModel twoFactorWriteModel,
        IMapper mapper)
    {
        _twoFactorReadModel = twoFactorReadModel;
        _twoFactorWriteModel = twoFactorWriteModel;
        _mapper = mapper;
    }
    public async Task<Domain.DomainEntities.TwoFactor?> GetByIdentifier(string email)
    {
        var twoFactorPersistence =  await _twoFactorReadModel.GetByIdentifier(email);
        if (twoFactorPersistence == null)
            return null;

        var twoFactor = _mapper.Map<Domain.DomainEntities.TwoFactor>(twoFactorPersistence);
        return twoFactor;

    }

    public async Task Delete(string id)
    {
        await _twoFactorWriteModel.Delete(ObjectId.Parse(id));
    }

    public async Task Insert(Domain.DomainEntities.TwoFactor twoFactor)
    {
        var twoFactorPersistence = _mapper.Map<TwoFactorPersistence>(twoFactor);
        await _twoFactorWriteModel.Insert(twoFactorPersistence);
    }
}