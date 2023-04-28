using AutoMapper;
using Domain.RepositoryInterfaces;
using MongoDB.Bson;
using Repository.User.UserPersistence;
using Repository.User.UserReadModel;
using Repository.User.UserWriteModel;

namespace Repository.User.UserRepository;

public class UserRepository : IUserRepository
{

    private readonly IMapper _mapper;
    private readonly IUserReadModel _userReadModel;
    private readonly IUserWriteModel _userWriteModel;
    
    public UserRepository(
        IMapper mapper,
        IUserReadModel userReadModel,
        IUserWriteModel userWriteModel)
    {
        _mapper = mapper;
        _userReadModel = userReadModel;
        _userWriteModel = userWriteModel;
    }

    public async Task<Domain.DomainEntities.User?> GetUserById(string id)
    {
        var user = await _userReadModel.GetUserById(ObjectId.Parse(id));
        if (user == null)
            throw new Exception(); // TODO: Fix this with real exceptions
        
        var response = _mapper.Map<UserPersistenceModel, Domain.DomainEntities.User>(user);
        return response;
    }

    public async Task<Domain.DomainEntities.User?> GetUserByEmail(string email)
    {
        var user = await _userReadModel.GetUserByEmail(email);
        if (user == null)
            throw new Exception(); // TODO: Fix this with real exceptions
        
        var response = _mapper.Map<UserPersistenceModel, Domain.DomainEntities.User>(user);
        return response;
    }

    public async Task<Domain.DomainEntities.User?> GetByRefreshToken(string token)
    {
        var user = await _userReadModel.GetByRefreshToken(token);
        return user != null ? _mapper.Map<Domain.DomainEntities.User>(user) : null;
    }

    public async Task UpdateUser(Domain.DomainEntities.User user)
    {
        var persistenceUser = _mapper.Map<UserPersistenceModel>(user);
        await _userWriteModel.UpdateUser(persistenceUser);
    }

    public async Task CreateUser(Domain.DomainEntities.User user)
    {
        var persistenceUser = _mapper.Map<UserPersistenceModel>(user);
        await _userWriteModel.InsertUser(persistenceUser);
    }
}