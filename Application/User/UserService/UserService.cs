﻿using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Helper;
using Application.JWT.Model;
using Application.User.Model;
using AutoMapper;
using Domain.DomainEntities;
using Domain.RepositoryInterfaces;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Application.User.UserService;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserAuthenticationService _userAuthentication;
    private readonly ITwoFactorRepository _twoFactorRepository;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public UserService(
        IMapper mapper,
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IUserAuthenticationService userAuthentication,
        ITwoFactorRepository twoFactorRepository,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _userAuthentication = userAuthentication;
        _twoFactorRepository = twoFactorRepository;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<Domain.DomainEntities.User?> GetUserByEmail(string email)
    {
        return await _userRepository.GetUserByEmail(email);
    }

    public async Task<Domain.DomainEntities.User?> GetByRefreshToken(string token)
    {
        return await _userRepository.GetByRefreshToken(token);
    }

    public async Task UpdateUser(Domain.DomainEntities.User user)
    {
        await _userRepository.UpdateUser(user);
    }

    public async Task<AdminAndOrganizationCreateResponse> CreateAdminUserAndOrganization(CreateAdminAndOrganizationRequest request)
    {
        if (request.Password.Length < 8)
            throw new ApplicationException("Password too short");

        var userToInsert = new Domain.DomainEntities.User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = request.Email,
            FullName = request.FullName,
            AccessLevels = new List<UserAccessLevel>
                {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
            RegistrationDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        var organizationToInsert = new Domain.DomainEntities.Organization
        {
            Id = ObjectId.GenerateNewId().ToString(),
            OrganizationName = request.OrganizationName,
            Country = request.Country,
            Address = request.Address,
            Dashboards = new List<OrganizationDashboards>(),
            Members = new List<OrganizationMembers>(),
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };

        userToInsert.OwnedOrganizationId = organizationToInsert.Id;
        organizationToInsert.OrganizationOwnerId = userToInsert.Id;
        organizationToInsert.Members.Add(new OrganizationMembers
            {
                Id = userToInsert.Id,
                Email = userToInsert.Email,
                Name = userToInsert.FullName,
                AccessLevel = userToInsert.AccessLevels
            });
        
        userToInsert.PasswordHash = PasswordHelper.GetHashedPassword(request.Password);
        await _userRepository.CreateUser(userToInsert);
        await _organizationRepository.Insert(organizationToInsert);

        return new AdminAndOrganizationCreateResponse
        {
            UserId = userToInsert.Id,
            Email = userToInsert.Email,
            UserName = userToInsert.FullName,
            AccessLevels = userToInsert.AccessLevels,
            UserCreationTime = userToInsert.RegistrationDate,

            OrganizationId = organizationToInsert.Id,
            OrganizationName = organizationToInsert.OrganizationName,
            Country = organizationToInsert.Country,
            Address = organizationToInsert.Address,
            OrganizationCreationTime = organizationToInsert.CreationDate
        };

    }

    public async Task Revoke(LogoutRequest request)
    {
        // Accept refresh token either from cookies or request body
        var token = request.Token ?? _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            // TODO: Custom exceptions
            throw new ApplicationException("Token is required");
        

        await _userAuthentication.RevokeToken(token);
    }

    public async Task<UserChangeAccessResponse> SetAccessLevel(ChangeUserAccessRequest request)
    {
        // We could could try to get admin user by refresh token that could be in the cookies, but if cookies are disable that might be a problem
        var adminUser = await _userRepository.GetUserById(request.AdminUserId);
        if (adminUser == null || !adminUser.AccessLevels.Contains(UserAccessLevel.Admin))
            // TODO: Use overwritten exceptions
            throw new ApplicationException("Admin user was not found");
        
        if (!adminUser.AccessLevels.Contains(UserAccessLevel.Admin))
            // TODO: Use overwritten exceptions
            throw new ApplicationException("Admin user seems that it does not have admin privileges");
        
        var user = await _userRepository.GetUserById(request.UserId);
        if (user == null)
            // TODO: Use overwritten exceptions
            throw new ApplicationException("User not found");
        
        if(adminUser.OwnedOrganizationId != user.MemberOfOrganizationId || adminUser.MemberOfOrganizationId != user.MemberOfOrganizationId)
            // TODO: Use overwritten exceptions
            throw new ApplicationException("Admin and user are not in the same organization");

        //TODO: Stop wanted set access level is the same as the user has
        
        user.AccessLevels = request.SetAccessLevel switch
        {
            UserAccessLevel.Admin => new List<UserAccessLevel> {UserAccessLevel.Admin, UserAccessLevel.Editor, UserAccessLevel.Viewer},
            UserAccessLevel.Editor => new List<UserAccessLevel> {UserAccessLevel.Editor, UserAccessLevel.Viewer},
            UserAccessLevel.Viewer => new List<UserAccessLevel> {UserAccessLevel.Viewer},
            _ => throw new Exception("Access level does not exist") // TODO: Use overwritten exceptions
        };

        await _userRepository.UpdateUser(user);
        var response = new UserChangeAccessResponse
        {
            UserId = user.Id,
            Email = user.Email,
            AccessLevels = user.AccessLevels
        };
        return response;
    }

    public async Task<Repository.TwoFactor.TwoFactorPersistence> ForgotPassword(ForgotPasswordRequest request)
    {
        var userTask = _userRepository.GetUserByEmail(request.Email);
        var twoFactorTask = _twoFactorRepository.GetByIdentifier(request.Email);
        await Task.WhenAll(userTask, twoFactorTask);
        var user = await userTask;
        var twoFactor = await twoFactorTask;

        if (user == null) 
            //TODO: User overwritten exceptions
            throw new ApplicationException("User not found");

        var generator = new Random();
        var token = generator.Next(0, 1000000).ToString("D6");

        if (twoFactor != null)
            await _twoFactorRepository.Delete(twoFactor.Id);

        var twoFactorToInsert = new TwoFactor
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = user.Id,
            Identifier = request.Email,
            ConfirmationCode = token,
            ConfirmationCreationDate = DateTime.UtcNow
        };

        await _twoFactorRepository.Insert(twoFactorToInsert);

        var response = await _emailService.SendEmail(request.Email, token);
        
        return _mapper.Map<Repository.TwoFactor.TwoFactorPersistence>(twoFactorToInsert);

    }
}