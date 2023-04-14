using System.Net;
using Application.Email.EmailService;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;
using WebApi.Controllers.V1.Email.Model;


namespace WebApi.Controllers.V1.Email;

// [Authorize]
[Route("api/v1/[controller]")]

public class EmailController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public EmailController(
        IMapper mapper,
        IEmailService emailService)
    {
        _mapper = mapper;
        _emailService = emailService;
    }

    /// <summary>
    /// Send an email
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    //[Authorization("Admin", "Editor", "Viewer")]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Send Email")]
    [Route("")]
    public async Task<IActionResult> SendEmail(SendEmailRequest request)
    {
        await _emailService.SendEmail(_mapper.Map<Application.Email.Model.SendEmailRequest>(request));
        // Exception Response from BaseExceptionHandlingService
        return Ok();
    }
    
}





/* Test user on https://ethereal.email/
Name	    Kobe Koch
Username	kobe.koch7@ethereal.email (also works as a real inbound email address)
Password	YbUvrku3cse2Uy4r1e  
 */