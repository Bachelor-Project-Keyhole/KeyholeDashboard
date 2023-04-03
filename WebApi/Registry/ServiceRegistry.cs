using Application.Email.EmailService;
using Application.JWT.Service;
using Domain.Repository.UserRepository;
using MongoDB.Driver;
using Repository.User.UserPersistence.ReadModel;
using Repository.User.UserPersistence.WriteModel;
using Repository.User.UserRepository;

namespace WebApi.Registry;

static class ServiceRegistry
{
    public static void RegisterPersistence(this IServiceCollection collection, IMongoDatabase database)
    {
        

        #region Application Layer

        collection.AddTransient<IEmailService, EmailService>();

        #endregion

        #region Repository Layer

        collection.AddSingleton<IUserReadModel>(new UserReadModel(database));
        collection.AddSingleton<IUserWriteModel>(new UserWriteModel(database));
        collection.AddTransient<IUserRepository, UserRepository>();

        #endregion
        
        
        collection.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();


    }
}