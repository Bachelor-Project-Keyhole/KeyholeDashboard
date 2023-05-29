using Application.Email.Helper;
using Application.Email.Template;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Application.Email.EmailService;

public class EmailService : IEmailService
{
    private readonly EmailAuth _email;

    public EmailService(
        IOptions<EmailAuth> mailkit)
    {
        _email = mailkit.Value;

    }

    public async Task<string> SendPasswordRecoveryTokenEmail(string toEmail, string link)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_email.SupportEmail)); // Just to test it out
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Test Email Subject";
            //TODO: Create html email body template in html and css 
            email.Body = new TextPart(TextFormat.Html) // Can be changed to different types of text format
            {
                // integrate html reading
                Text = EmailTemplate.InviteUser($"Password recovery link email. To proceed click button below",
                    link)
            };

            using var smpt = new SmtpClient(); // Use mailKit instead of system package.

            // For example if you want to use gmail, the parameter would be something like "smtp.gmail.com"
            //TODO: Later on password should be stored to azure vault.
            await smpt.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smpt.AuthenticateAsync(_email.SupportEmail, _email.EmailPassword);
            await smpt.SendAsync(email);
            await smpt.DisconnectAsync(true);
            return "Mail was sent";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "Something went wrong, mail was not sent";
        }
    }
    
    public async Task<string> SendInvitationEmail(string toEmail, string? message, string link, string organizationName)
    {
        try
        {
            if (string.IsNullOrEmpty(message))
                message ??= $"You have been invited to join organization: {organizationName}." +
                            $" To complete the registration continue with the given link: {link}";
            else
                message += " " + link;
            
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_email.SupportEmail)); // Just to test it out
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Test Email Subject";
            //TODO: Create html email body template in html and css 
            email.Body = new TextPart(TextFormat.Html) // Can be changed to different types of text format
            {
                // integrate html reading
                Text = EmailTemplate.InviteUser(message, link)
            };

            using var smpt = new SmtpClient(); // Use mailKit instead of system package.

            // For example if you want to use gmail, the parameter would be something like "smtp.gmail.com"
            //TODO: Later on password should be stored to azure vault.
            await smpt.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smpt.AuthenticateAsync(_email.SupportEmail, _email.EmailPassword);
            await smpt.SendAsync(email);
            await smpt.DisconnectAsync(true);
            return "Mail was sent";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "Something went wrong, mail was not sent";
        }
        
    }
}