using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Service;
using Application.Organization;
using Application.User.UserService;
using Domain.Datapoint;
using Domain.Organization;
using Domain.RepositoryInterfaces;
using Domain.Template;
using Repository.Datapoint;
using Repository.Organization;
using Repository.OrganizationUserInvite;
using Repository.TwoFactor;
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
        collection.AddTransient<IOrganizationService, OrganizationService>();

        #endregion
        
        #region Repository Layer
        
        collection.AddTransient<IUserRepository, UserRepository>();
        collection.AddTransient<ITwoFactorRepository, TwoFactorRepository>();
        collection.AddTransient<IOrganizationUserInviteRepository, OrganizationUserInviteRepository>();


        #endregion
        
        collection.AddTransient<IOrganizationRepository, OrganizationRepository>();
        collection.AddTransient<IOrganizationDomainService, OrganizationDomainService>();
        collection.AddTransient<IDataPointEntryRepository, DataPointEntryRepository>();
        collection.AddTransient<IDataPointRepository, DataPointRepository>();
        collection.AddTransient<IDataPointDomainService, DataPointDomainService>();
        collection.AddTransient<ITemplateDomainService, TemplateDomainService>();
    }
}