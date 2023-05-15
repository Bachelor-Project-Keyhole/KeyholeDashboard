using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Service;
using Application.User.UserService;
using Domain.Datapoint;
using Domain.RepositoryInterfaces;
using Repository.Datapoint;
using Repository.Organization;
using Repository.TwoFactor;
using Repository.TwoFactor.TwoFactorReadModel;
using Repository.TwoFactor.TwoFactorWriteModel;
using Repository.User.UserRepository;

namespace WebApi.Registry;

static class ServiceRegistry
{
    public static void RegisterPersistence(this IServiceCollection collection)
    {

        #region Application-Service Layer
        
        collection.AddTransient<IUserService, UserService>();
        collection.AddTransient<IEmailService, EmailService>();
        collection.AddTransient<IUserAuthenticationService, UserAuthenticationService>();
        collection.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
        
        #endregion
        
        #region Repository Layer
        
        collection.AddTransient<IUserRepository, UserRepository>();
        collection.AddTransient<ITwoFactorRepository, TwoFactorRepository>();
        

        #endregion
        
        collection.AddTransient<IOrganizationRepository, OrganizationRepository>();
        collection.AddTransient<IDataPointEntryRepository, DataPointEntryRepository>();
        collection.AddTransient<IDataPointRepository, DataPointRepository>();
        collection.AddTransient<IDataPointDomainService, DataPointDomainService>();
    }
}