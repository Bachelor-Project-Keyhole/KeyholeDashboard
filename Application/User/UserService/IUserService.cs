using Application.JWT.Model;
using Application.Organization.Model;
using Application.User.Model;

namespace Application.User.UserService;

public interface IUserService
{
    Task<Domain.User.User?> GetUserById(string id);
    Task<Domain.User.User?> GetUserByEmail(string email);
    Task<Domain.User.User?> GetByRefreshToken(string token);
    Task UpdateUser(Domain.User.User user);
    Task<AdminAndOrganizationCreateResponse> CreateAdminUserAndOrganization(CreateAdminAndOrganizationRequest request);
    Task Revoke(LogoutRequest request);
    Task<UserChangeAccessResponse> SetAccessLevel(ChangeUserAccessRequest request);
    Task<Repository.TwoFactor.TwoFactorPersistence> ForgotPassword(ForgotPasswordRequest request);
}