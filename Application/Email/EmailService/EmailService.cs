﻿using Application.Email.Helper;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Application.Email.EmailService;

public class EmailService : IEmailService
{
    private readonly EmailAuth _email;
    private readonly IUrlHelper _urlHelper;

    public EmailService(
        IOptions<EmailAuth> mailkit,
        IUrlHelper urlHelper)
    {
        _email = mailkit.Value;
        _urlHelper = urlHelper;
    }
    
    public async Task<string> SendEmail(string toEmail, string emailBody)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_email.SupportEmail)); // Just to test it out
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Test Email Subject";
            var registrationLink = _urlHelper.Action("InviteUserToOrganization", "Organization");
            //TODO: Create html email body template in html and css 
            email.Body = new TextPart(TextFormat.Html) // Can be changed to different types of text format
            {
                // integrate html reading
                Text = emailBody
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