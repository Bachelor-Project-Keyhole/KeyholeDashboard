using Application.Email.EmailService;
using Application.User.UserService;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Shared;


namespace WebApi.Controllers.V1.Email;

// [Authorize]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
public class EmailController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;

    public EmailController(
        IMapper mapper,
        IEmailService emailService,
        IUserService userService)
    {
        _mapper = mapper;
        _emailService = emailService;
        _userService = userService;
    }

    




}





/* Test user on https://ethereal.email/
Name	    Kobe Koch
Username	kobe.koch7@ethereal.email (also works as a real inbound email address)
Password	YbUvrku3cse2Uy4r1e  
 */