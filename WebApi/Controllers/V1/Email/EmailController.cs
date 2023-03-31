using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Service;
using Service.Email.EmailService;
using Service.ExceptionHandling;
using Service.ExceptionHandling.BaseExceptionHandlingService;
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
    private readonly IBaseExceptionHandlingService _baseException;

    public EmailController(
        IMapper mapper,
        IEmailService emailService,
        IBaseExceptionHandlingService baseException)
    {
        _mapper = mapper;
        _emailService = emailService;
        _baseException = baseException;
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
        var status = await _emailService.SendEmail(_mapper.Map<Service.Email.Model.SendEmailRequest>(request));
        // Exception Response from BaseExceptionHandlingService
        return Ok();
    }

}



/* Test user on https://ethereal.email/
Name	    Kobe Koch
Username	kobe.koch7@ethereal.email (also works as a real inbound email address)
Password	YbUvrku3cse2Uy4r1e  
 */