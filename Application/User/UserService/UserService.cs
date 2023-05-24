using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Helper;
using Application.User.Model;
using AutoMapper;
using Contracts.v1.Authentication;
using Contracts.v1.Organization;
using Domain;
using Domain.Exceptions;
using Domain.RepositoryInterfaces;
using Domain.TwoFactor;
using Domain.User;
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
        IHttpContextAccessor httpContextAccessor
        )
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _userAuthentication = userAuthentication;
        _twoFactorRepository = twoFactorRepository;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Domain.User.User?> GetUserById(string id)
    {
        var user = await _userRepository.GetUserById(id);
        if (user == null)
            throw new UserNotFoundException($"User with given email: {id} was not found");
        return user;
    }

    public async Task<Domain.User.User?> GetUserByEmail(string email)
    {
        var user = await _userRepository.GetUserByEmail(email);
        if (user == null)
            throw new UserNotFoundException($"User with given email: {email} was not found");
        return user;
    }

    public async Task<Domain.User.User?> GetByRefreshToken(string token)
    {
        return await _userRepository.GetByRefreshToken(token);
    }

    public async Task<AllUsersOfOrganizationResponse> GetAllUsers(string organizationId)
    {
        var users = await _userRepository.GetAllUsersByOrganizationId(organizationId);
        if (users == null)
            throw new UserNotFoundException($"No users were found with organization Id: {organizationId}");

        return new AllUsersOfOrganizationResponse
        {
            OrganizationId = organizationId,
            Users = _mapper.Map<List<OrganizationUsersResponse>>(users)
        };
    }

    public async Task UpdateUser(Domain.User.User user)
    {
        await _userRepository.UpdateUser(user);
    }

    public async Task RemoveUserById(string userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
            throw new UserNotFoundException($"user with id: {userId} was not found");
        if (user.OwnedOrganizationId != null)
            throw new UserForbiddenAction($"user with id: {userId} is an owner");

        await _userRepository.RemoveUserById(userId);
    }

    public async Task<AdminAndOrganizationCreateResponse> CreateAdminUserAndOrganization(CreateAdminAndOrganizationRequest request)
    {
        if (request.Password.Length < 8)
            throw new PasswordTooShortException("Password too short");
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user != null)
            throw new UserEmailTakenException($"This email: {request.Email} is already taken");

        var userToInsert = new Domain.User.User
        {
            Id = IdGenerator.GenerateId(),
            Email = request.Email,
            FullName = request.FullName,
            AccessLevels = new List<UserAccessLevel>
                {UserAccessLevel.Viewer, UserAccessLevel.Editor, UserAccessLevel.Admin},
            RegistrationDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        var organizationToInsert = new Domain.Organization.Organization
        {
            Id = IdGenerator.GenerateId(),
            OrganizationName = request.OrganizationName,
            ApiKey = IdGenerator.GenerateId(),
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };

        userToInsert.MemberOfOrganizationId = organizationToInsert.Id;
        userToInsert.OwnedOrganizationId = organizationToInsert.Id;
        organizationToInsert.OrganizationOwnerId = userToInsert.Id;
        userToInsert.PasswordHash = PasswordHelper.GetHashedPassword(request.Password);
        await _userRepository.CreateUser(userToInsert);
        await _organizationRepository.Insert(organizationToInsert);

        return new AdminAndOrganizationCreateResponse
        {
            UserId = userToInsert.Id,
            Email = userToInsert.Email,
            UserName = userToInsert.FullName,
            AccessLevels = userToInsert.AccessLevels,
            UserCreationTime = userToInsert.RegistrationDate.ToLocalTime(),

            OrganizationId = organizationToInsert.Id,
            OrganizationName = organizationToInsert.OrganizationName,
            OrganizationCreationTime = organizationToInsert.CreationDate.ToLocalTime()
        };

    }

    public async Task<UserRegistrationResponse> CreateUser(string organizationId, string email, List<UserAccessLevel> accessLevels, UserRegistrationRequest request)
    {
        if (request.Password.Length < 8)
            throw new ApplicationException("Password too short");

        var user = await _userRepository.GetUserByEmail(email);
        if (user != null)
            throw new UserEmailTakenException($"This email: {email} is already taken");
        
        var userToInsert = new Domain.User.User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = email,
            FullName = request.FullName,
            AccessLevels = accessLevels,
            OwnedOrganizationId = null,
            MemberOfOrganizationId = organizationId,
            RegistrationDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            PasswordHash = PasswordHelper.GetHashedPassword(request.Password)
        };

        await _userRepository.CreateUser(userToInsert);

        return new UserRegistrationResponse
        {
            Email = userToInsert.Email,
            FullName = userToInsert.FullName,
            OrganizationId = userToInsert.MemberOfOrganizationId,
            RegistrationDate = userToInsert.RegistrationDate.ToLocalTime().ToString("f")
        };
        
    }
    
    public async Task Revoke(LogoutRequest request)
    {
        // Accept refresh token either from cookies or request body
        var token = request.Token ?? _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            throw new RevokeTokenBadRequest("Token is required");
    
        await _userAuthentication.RevokeToken(token);
    }

    public async Task<UserChangeAccessResponse> SetAccessLevel(ChangeUserAccessRequest request)
    {
        // We could could try to get admin user by refresh token that could be in the cookies, but if cookies are disable that might be a problem
        var adminUser = await _userRepository.GetUserById(request.AdminUserId);
        if (adminUser == null)
            throw new UserNotFoundException("Admin user was not found");
            
        
        var user = await _userRepository.GetUserById(request.UserId);
        if (user == null)
            throw new UserNotFoundException("User not found");

        if (!string.IsNullOrEmpty(user.OwnedOrganizationId))
            throw new AccessLevelForbiddenException("Owner cannot be removed.");

        if (adminUser.MemberOfOrganizationId != user.MemberOfOrganizationId)
            throw new UserInvalidActionException("Admin and user are not in the same organization");
            

   
        
        user.AccessLevels = request.SetAccessLevel switch
        {
            UserAccessLevel.Admin => new List<UserAccessLevel> {UserAccessLevel.Admin, UserAccessLevel.Editor, UserAccessLevel.Viewer},
            UserAccessLevel.Editor => new List<UserAccessLevel> {UserAccessLevel.Editor, UserAccessLevel.Viewer},
            UserAccessLevel.Viewer => new List<UserAccessLevel> {UserAccessLevel.Viewer},
            _ => throw new AccessLevelForbiddenException("Access level does not exist")
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

    public async Task<string> ForgotPassword(ForgotPasswordRequest request)
    {
        var userTask = _userRepository.GetUserByEmail(request.Email);
        var twoFactorTask = _twoFactorRepository.GetByIdentifier(request.Email);
        await Task.WhenAll(userTask, twoFactorTask);
        var user = await userTask;
        var twoFactor = await twoFactorTask;

        if (user == null) 
            throw new UserNotFoundException($"User was not found with email: {request.Email}");

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

        var response = await _emailService.SendPasswordRecoveryTokenEmail(request.Email, token);

        return response;
    }
}