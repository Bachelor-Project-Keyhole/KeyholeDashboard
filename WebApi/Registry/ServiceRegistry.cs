﻿using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Service;
using Application.User.UserService;
using Domain.RepositoryInterfaces;
using MongoDB.Driver;
using Repository.Organization;
using Repository.Organization.OrganizationReadModel;
using Repository.Organization.OrganizationWriteModel;
using Repository.TwoFactor;
using Repository.TwoFactor.TwoFactorReadModel;
using Repository.TwoFactor.TwoFactorWriteModel;
using Repository.User.UserReadModel;
using Repository.User.UserRepository;
using Repository.User.UserWriteModel;

namespace WebApi.Registry;

static class ServiceRegistry
{
    public static void RegisterPersistence(this IServiceCollection collection, IMongoDatabase database)
    {

        #region Application-Service Layer

        collection.AddTransient<IEmailService, EmailService>();
        collection.AddTransient<IUserAuthenticationService, UserAuthenticationService>();
        collection.AddTransient<IUserService, UserService>();
        collection.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();

        

        #endregion
        
        #region Repository Layer

        
        
        collection.AddTransient<IUserRepository, UserRepository>();
        collection.AddSingleton<IUserReadModel>(new UserReadModel(database));
        collection.AddSingleton<IUserWriteModel>(new UserWriteModel(database));
        
        collection.AddTransient<ITwoFactorRepository, TwoFactorRepository>();
        collection.AddSingleton<ITwoFactorReadModel>(new TwoFactorReadModel(database));
        collection.AddSingleton<ITwoFactorWriteModel>(new TwoFactorWriteModel(database));
        
        collection.AddTransient<IOrganizationRepository, OrganizationRepository>();
        collection.AddSingleton<IOrganizationReadModel>(new OrganizationReadModel(database));
        collection.AddSingleton<IOrganizationWriteModel>(new OrganizationWriteModel(database));
        
        #endregion
    }
}