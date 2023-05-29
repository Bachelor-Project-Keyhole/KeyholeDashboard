using Application.Authentication.AuthenticationService;
using Application.Email.EmailService;
using Application.JWT.Helper;
using AutoMapper;
using Contracts.v1.Authentication;
using Contracts.v1.Organization;
using Contracts.v1.User;
using Domain;
using Domain.Exceptions;
using Domain.Organization;
using Domain.RepositoryInterfaces;
using Domain.TwoFactor;
using Domain.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Application.User.UserService;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserDomainService _userDomainService;
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationDomainService _organizationDomainService;
    private readonly IUserAuthenticationService _userAuthentication;
    private readonly ITwoFactorRepository _twoFactorRepository;
    private readonly ITwoFactorDomainService _twoFactorDomainService;
    private readonly UserPasswordResetRoute _userPasswordResetRoute;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public UserService(
        IMapper mapper,
        IUserDomainService userDomainService,
        IUserRepository userRepository,
        IOrganizationDomainService organizationDomainService,
        IUserAuthenticationService userAuthentication,
        ITwoFactorRepository twoFactorRepository,
        ITwoFactorDomainService twoFactorDomainService,
        IOptions<UserPasswordResetRoute> userPasswordResetRoute,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _mapper = mapper;
        _userDomainService = userDomainService;
        _userRepository = userRepository;
        _organizationDomainService = organizationDomainService;
        _userAuthentication = userAuthentication;
        _twoFactorRepository = twoFactorRepository;
        _twoFactorDomainService = twoFactorDomainService;
        _userPasswordResetRoute = userPasswordResetRoute.Value;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Domain.User.User?> GetUserById(string id)
    {
        var user = await _userDomainService.GetUserById(id);
        return user;
    }
    
    public async Task<AllUsersOfOrganizationResponse> GetAllUsers(string organizationId)
    {
        var users = await _userDomainService.GetAllUsersByOrganizationId(organizationId);
        return new AllUsersOfOrganizationResponse
        {
            OrganizationId = organizationId,
            Users = _mapper.Map<List<OrganizationUsersResponse>>(users)
        };
    }

    public async Task RemoveUserById(string userId)
    {
        var user = await _userDomainService.GetUserById(userId);
        if (user.OwnedOrganizationId != null)
            throw new UserForbiddenAction($"user with id: {userId} is an owner");

        await _userDomainService.RemoveUserById(userId);
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
        await _userDomainService.CreateUser(userToInsert);
        await _organizationDomainService.Insert(organizationToInsert);

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

        await _userDomainService.CreateUser(userToInsert);

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

        var user = await _userDomainService.GetUserById(request.UserId);

        if (!string.IsNullOrEmpty(user.OwnedOrganizationId))
            throw new AccessLevelForbiddenException("Owner cannot be removed.");



        var toEnum = Enum.TryParse(request.SetAccessLevel, out UserAccessLevel accessLevel);
        if(toEnum == false)
            throw new AccessLevelForbiddenException($"This access level does not exist: {request.SetAccessLevel}");


        user.AccessLevels = accessLevel switch
        {
            UserAccessLevel.Admin => new List<UserAccessLevel> {UserAccessLevel.Admin, UserAccessLevel.Editor, UserAccessLevel.Viewer},
            UserAccessLevel.Editor => new List<UserAccessLevel> {UserAccessLevel.Editor, UserAccessLevel.Viewer},
            UserAccessLevel.Viewer => new List<UserAccessLevel> {UserAccessLevel.Viewer},
            _ => throw new AccessLevelForbiddenException("Access level does not exist")
        };

        await _userDomainService.UpdateUser(user);
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
        var userTask = _userDomainService.GetUserByEmail(request.Email);
        var twoFactorTask = _twoFactorRepository.GetByIdentifier(request.Email);
        await Task.WhenAll(userTask, twoFactorTask);
        var user = await userTask;
        var twoFactor = await twoFactorTask;

        if (user == null) 
            throw new UserNotFoundException($"User was not found with email: {request.Email}");


        var token = GenerateAlphaNumeric();
        var link = $"{_userPasswordResetRoute.Link}/{token}";

        if (twoFactor != null)
            await _twoFactorRepository.DeleteById(twoFactor.Id);

        var twoFactorToInsert = new TwoFactor
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = user.Id,
            Identifier = request.Email,
            ConfirmationCode = token,
            ConfirmationCreationDate = DateTime.UtcNow
        };

        await _twoFactorRepository.Insert(twoFactorToInsert);
        return await _emailService.SendPasswordRecoveryTokenEmail(request.Email, link);
    }

    public async Task ResetPassword(string token, string password)
    {
        var twoFactor = await _twoFactorDomainService.GetByToken(token);
        if (twoFactor.ConfirmationCreationDate.AddMinutes(10) < DateTime.UtcNow)
        {
            await _twoFactorDomainService.DeleteByToken(token);
            throw new TokenExpiredException("Token has expired and was removed");
        }
        var user = await _userDomainService.GetUserById(twoFactor.UserId); // verify that user exists
        
        if (password.Length < 8)
            throw new PasswordTooShortException("Password too short");
        
        user.PasswordHash = PasswordHelper.GetHashedPassword(password);
        await _userDomainService.UpdateUser(user);
        await _twoFactorDomainService.DeleteById(twoFactor.Id);
    }
    
    private string GenerateAlphaNumeric()
    {
        var length = 6;
        Random random = new Random();
        string charCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => charCharacters[random.Next(charCharacters.Length)])
            .ToArray());
    }
}