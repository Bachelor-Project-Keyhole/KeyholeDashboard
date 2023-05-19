using Application.JWT.Model;
using Application.Organization.Model;
using Application.User.Model;
using Repository.User.UserPersistence;

namespace Application.User.UserService;

public interface IUserService
{
    Task<Domain.User.User?> GetUserById(string id);
    Task<Domain.User.User?> GetUserByEmail(string email);
    Task<Domain.User.User?> GetByRefreshToken(string token);
    Task<AllUsersOfOrganizationResponse> GetAllUsers(string organizationId);
    Task UpdateUser(Domain.User.User user);
    Task RemoveUserById(string userId);
    Task<AdminAndOrganizationCreateResponse> CreateAdminUserAndOrganization(CreateAdminAndOrganizationRequest request);
    Task<UserRegistrationResponse> CreateUser(string organizationId, string email, List<Domain.User.UserAccessLevel> accessLevels, UserRegistrationRequest request);
    Task Revoke(LogoutRequest request);
    Task<UserChangeAccessResponse> SetAccessLevel(ChangeUserAccessRequest request);
    Task<Repository.TwoFactor.TwoFactorPersistence> ForgotPassword(ForgotPasswordRequest request);
}