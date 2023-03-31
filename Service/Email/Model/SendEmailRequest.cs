#pragma warning disable CS8618
namespace Service.Email.Model;

public class SendEmailRequest
{
    public string ToEmail { get; set; }
    public string EmailBody { get; set; }
}