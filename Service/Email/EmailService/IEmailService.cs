using Service.Email.Model;

namespace Service.Email.EmailService;

public interface IEmailService
{
    Task<string> SendEmail(SendEmailRequest request);
}