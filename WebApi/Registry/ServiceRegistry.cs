using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Service;
using Domain.UserRepository;
using MongoDB.Driver;
using Repository.User.UserReadModel;
using Repository.User.UserRepository;
using Repository.User.UserWriteModel;

namespace WebApi.Registry;

static class ServiceRegistry
{
    public static void RegisterPersistence(this IServiceCollection collection, IMongoDatabase database)
    {
        

        #region Application Layer

        // Email
        collection.AddTransient<IEmailService, EmailService>();

        // User Authentication
        collection.AddTransient<IUserAuthenticationService, UserAuthenticationService>();
        
        #endregion

        #region Repository Layer

        collection.AddSingleton<IUserReadModel>(new UserReadModel(database));
        collection.AddSingleton<IUserWriteModel>(new UserWriteModel(database));
        collection.AddTransient<IUserRepository, UserRepository>();

        #endregion
        
        
        collection.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();


    }
}