using AutoMapper;
using Domain.RepositoryInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Repository.User.UserPersistence;

namespace Repository.User.UserRepository;

public class UserRepository : MongoRepository<UserPersistenceModel>, IUserRepository
{

    private readonly IMapper _mapper;

    public UserRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper) : base(dataBaseOptions)
    {
        _mapper = mapper;
    }

    public async Task<Domain.User.User?> GetUserById(string id)
    {
        var user = await FindOneAsync(x => x.Id == ObjectId.Parse(id));
        var response = _mapper.Map<Domain.User.User>(user);
        return response;
    }

    public async Task<Domain.User.User?> GetUserByEmail(string email)
    {
        var user = await FindOneAsync(x => x.Email == email);
        var response = _mapper.Map<Domain.User.User>(user);
        return response;
    }

    public async Task<Domain.User.User?> GetByRefreshToken(string token)
    {
        var user = await FindOneAsync(x => x.RefreshTokens!.Any(y => y.Token == token));
        return user != null ? _mapper.Map<Domain.User.User>(user) : null;
    }

    public async Task UpdateUser(Domain.User.User user)
    {
        var persistenceUser = _mapper.Map<UserPersistenceModel>(user);
        await ReplaceOneAsync(persistenceUser);
    }

    public async Task CreateUser(Domain.User.User user)
    {
        var persistenceUser = _mapper.Map<UserPersistenceModel>(user);
        await InsertOneAsync(persistenceUser);
    }
}