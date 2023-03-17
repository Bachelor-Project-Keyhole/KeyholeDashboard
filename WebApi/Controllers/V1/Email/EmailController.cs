using System.Net;
using AutoMapper;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Email;

// [Authorize]
[Route("api/v1/[controller]")]

public class EmailController : BaseApiController
{
    private readonly IMapper _mapper;

    public EmailController(
        IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Send an email
    /// </summary>
    /// <param name="toEmail"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Send Email")]
    [Route("")]
    public async Task<IActionResult> SendEmail(string toEmail, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("joe.dashboards@gmail.com")); // Just to test it out
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Test Email Subject";
        email.Body = new TextPart(TextFormat.Html)
        {
            Text = body
        };

        using var smpt = new SmtpClient(); // Use mailKit instead of system package.

        // For example if you want to use gmail, the parameter would be something like "smtp.gmail.com"
        //TODO: inject the password and email.
        await smpt.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await smpt.AuthenticateAsync("joe.dashboards@gmail.com", "dvmegeprkxucbnju");
        await smpt.SendAsync(email);
        await smpt.DisconnectAsync(true);

        return Ok();

    }

}



/* Test user on https://ethereal.email/
Name	    Kobe Koch
Username	kobe.koch7@ethereal.email (also works as a real inbound email address)
Password	YbUvrku3cse2Uy4r1e  
 */