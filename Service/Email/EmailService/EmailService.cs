﻿using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Service.Email.Helper;
using Service.Email.Model;

namespace Service.Email.EmailService;

public class EmailService : IEmailService
{
    private readonly EmailAuth _email;

    public EmailService(IOptions<EmailAuth> mailkit)
    {
        _email = mailkit.Value;
    }
    
    public async Task SendEmail(SendEmailRequest request)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_email.SupportEmail)); // Just to test it out
            email.To.Add(MailboxAddress.Parse(request.ToEmail));
            email.Subject = "Test Email Subject";
            // Create html email body template in html and css 
            email.Body = new TextPart(TextFormat.Html) // Can be changed to different types of text format
            {
                Text = request.EmailBody
            };

            using var smpt = new SmtpClient(); // Use mailKit instead of system package.

            // For example if you want to use gmail, the parameter would be something like "smtp.gmail.com"
            //TODO: Later on password should be stored to azure vault.
            await smpt.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smpt.AuthenticateAsync(_email.SupportEmail, _email.EmailPassword);
            await smpt.SendAsync(email);
            await smpt.DisconnectAsync(true);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}