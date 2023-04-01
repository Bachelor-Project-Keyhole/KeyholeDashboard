using Domain.Repository.UserRepository;
using Repository.UserRepository;
using Service.Email.EmailService;
using Service.JWT.Service;

namespace WebApi.Registry;

static class ServiceRegistry
{
    public static void RegisterPersistence(this IServiceCollection collection)
    {
        

        #region Service Layer

        collection.AddTransient<IEmailService, EmailService>();

        #endregion
        
        collection.AddTransient<IUserRepository, UserRepository>();
        
        collection.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();


    }
}