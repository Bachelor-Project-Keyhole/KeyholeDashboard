using Application.Email.Model;

namespace Application.Email.EmailService;

public interface IEmailService
{
    Task SendEmail(SendEmailRequest request);
}