using Domain.Exceptions;
using Domain.RepositoryInterfaces;

namespace Domain.TwoFactor;

public class TwoFactorDomainService : ITwoFactorDomainService
{
    
    private readonly ITwoFactorRepository _twoFactorRepository;

    public TwoFactorDomainService(ITwoFactorRepository twoFactorRepository)
    {
        _twoFactorRepository = twoFactorRepository;
    }
    
    public async Task<TwoFactor> GetByToken(string token)
    {
        var twoFactor = await _twoFactorRepository.GetByToken(token);
        if (twoFactor == null)
            throw new TokenNotFoundException("Token was not found");
        
        return twoFactor;
    }

    public async Task DeleteById(string tokenId)
    {
        await _twoFactorRepository.DeleteById(tokenId);
    }

    public async Task DeleteByToken(string token)
    {
        await _twoFactorRepository.DeleteByToken(token);
    }
}