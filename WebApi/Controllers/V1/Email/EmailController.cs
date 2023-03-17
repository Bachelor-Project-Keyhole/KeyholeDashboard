using System.Net;
using AutoMapper;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;
using WebApi.Controllers.V1.Email.Model;
using WebApi.Helper;
using WebApi.Services.MailKit;

namespace WebApi.Controllers.V1.Email;

// [Authorize]
[Route("api/v1/[controller]")]

public class EmailController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly IMailKitService _mailKitService;

    public EmailController(
        IMapper mapper,
        IMailKitService mailKitService)
    {
        _mapper = mapper;
        _mailKitService = mailKitService;
    }

    /// <summary>
    /// Send an email
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Send Email")]
    [Route("")]
    public async Task<IActionResult> SendEmail(SendEmailRequest request)
    {
        await _mailKitService.SendEmail(request);
        return Ok();
    }

}



/* Test user on https://ethereal.email/
Name	    Kobe Koch
Username	kobe.koch7@ethereal.email (also works as a real inbound email address)
Password	YbUvrku3cse2Uy4r1e  
 */