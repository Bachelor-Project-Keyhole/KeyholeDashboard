using Application.Email.Model;

namespace Application.Email.EmailService;

public interface IEmailService
{
    Task<string> SendEmail(string toEmail, string emailBody);
}