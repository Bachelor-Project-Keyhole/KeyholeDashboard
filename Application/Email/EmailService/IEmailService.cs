namespace Application.Email.EmailService;

public interface IEmailService
{
    Task<string> SendPasswordRecoveryTokenEmail(string toEmail, string token);
    Task<string> SendInvitationEmail(string toEmail, string? message, string link, string organizationName);
}