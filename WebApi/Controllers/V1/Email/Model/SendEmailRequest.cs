#pragma warning disable CS8618
namespace WebApi.Controllers.V1.Email.Model;

public class SendEmailRequest
{
    public string ToEmail { get; set; }
    public string EmailBody { get; set; }
}