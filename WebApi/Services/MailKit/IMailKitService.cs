using WebApi.Controllers.V1.Email.Model;

namespace WebApi.Services.MailKit;

public interface IMailKitService
{
    Task SendEmail(SendEmailRequest request);
}