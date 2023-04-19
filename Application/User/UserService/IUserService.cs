using Application.JWT.Model;
using Application.User.Model;

namespace Application.User.UserService;

public interface IUserService
{
    Task<Domain.DomainEntities.User?> GetUserByEmail(string email);
    Task<Domain.DomainEntities.User?> GetByRefreshToken(string token);
    Task UpdateUser(Domain.DomainEntities.User user);
    Task<AdminAndOrganizationCreateResponse> CreateAdminUserAndOrganization(CreateAdminAndOrganizationRequest request);
    Task Revoke(LogoutRequest request);
    Task<UserChangeAccessResponse> SetAccessLevel(ChangeUserAccessRequest request);
    Task<Repository.TwoFactor.TwoFactorPersistence> ForgotPassword(ForgotPasswordRequest request);
}