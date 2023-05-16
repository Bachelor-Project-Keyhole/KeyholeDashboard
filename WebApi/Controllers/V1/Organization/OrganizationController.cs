using System.Net;
using System.Transactions;
using Application.Email.EmailService;
using Application.JWT.Authorization;
using Application.Organization;
using Application.Organization.Model;
using Application.User.Model;
using Application.User.UserService;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Organization;


[Route("api/v1/[controller]")]
public class OrganizationController : BaseApiController
{
    private readonly IOrganizationService _organizationService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public OrganizationController(
        IOrganizationService organizationService,
        IUserService userService,
        IEmailService emailService)
    {
        _organizationService = organizationService;
        _userService = userService;
        _emailService = emailService;
    }
    
    /// <summary>
    /// Register an user alongside the organization
    /// </summary>
    /// <param name="request"> User and company registration parameters </param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Register an user alongside the organization", typeof(AdminAndOrganizationCreateResponse))]
    [Route("register")]
    public async Task<IActionResult> CreateAdminUser([FromBody] CreateAdminAndOrganizationRequest request)
    {
        var response = await _userService.CreateAdminUserAndOrganization(request); // Maybe it would be a good idea to separate into 2 endpoints.
        return Ok(response);
    }


    /// <summary>
    /// Invite user via email
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Invite user into the organization", typeof(OrganizationUserInviteResponse))]
    [Route("invite")]
    public async Task<IActionResult> InviteUserToOrganization(OrganizationUserInviteRequest request)
    {
        var link = await _organizationService.InviteUser(request);
        await _emailService.SendInvitationEmail(request.ReceiverEmailAddress, request.Message, link.Item1, link.Item2);
        return Ok(link.Item1);
    }

    /// <summary>
    /// Complete user registration (Note for now do not use the link and just pass the token from mail
    /// </summary>
    /// <param name="token"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "User registration completion", typeof(UserRegistrationResponse))]
    [Route("register/{token}")]
    public async Task<IActionResult> CompleteUserRegistration(string token, [FromBody] UserRegistrationRequest request)
    {
        // Define transaction
        using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                // Start
                var invitation = await _organizationService.TokenValidity(token);
                var response = await _userService.CreateUser(invitation.OrganizationId, invitation.ReceiverEmail, invitation.AccessLevels, request);
                
                // Compete transaction
                transactionScope.Complete();

                return Ok(response);
            }
            catch (Exception e)
            {
                // How do we handle this part? 
                transactionScope.Dispose();
                Console.WriteLine("Registration of a user was not successful \n" +  e);
                throw;
            }
        }
    }
    
}