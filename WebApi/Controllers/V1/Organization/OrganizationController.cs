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
    /// Register an user alongside the organization (No Auth required)
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
    /// Invite user via email (Admin Endpoint)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    //[AllowAnonymous]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Invite user into the organization")]
    [Route("invite")]
    public async Task<IActionResult> InviteUserToOrganization(OrganizationUserInviteRequest request)
    {
        var link = await _organizationService.InviteUser(request);
        await _emailService.SendInvitationEmail(request.ReceiverEmailAddress, request.Message, link.Item1, link.Item2);
        return Ok();
    }

    /// <summary>
    /// Complete user registration (Note for now do not use the link and just pass the token from mail (No auth required)
    /// </summary>
    /// <param name="token"> Token, that get attached to the end of a query </param>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
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

    /// <summary>
    /// Get all users of the organization (Any auth level required)
    /// </summary>
    /// <param name="organizationId"> id of an organization, the users are assigned to</param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin, UserAccessLevel.Editor, UserAccessLevel.Viewer)]
    //[AllowAnonymous] // For testing
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get all users of the organization", typeof(AllUsersOfOrganizationResponse))]
    [Route("users/{organizationId}")]
    public async Task<IActionResult> GetAllUsersOfOrganization(string organizationId)
    {
        var response = await _userService.GetAllUsers(organizationId);
        return Ok(response);
    }

    /// <summary>
    /// Update organization (Admin endpoint)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    //[AllowAnonymous] // For testing
    [HttpPut]
    [SwaggerResponse((int) HttpStatusCode.OK, "Update organization", typeof(OrganizationResponse))]
    [Route("update")]
    public async Task<IActionResult> UpdateOrganization([FromBody] UpdateOrganizationRequest request)
    {
        var response = await _organizationService.UpdateOrganization(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Change access level to user
    /// </summary>
    /// <param name="request">
    /// AdminId -> admin that wants to change access to user
    /// UserId -> id of user that accesses will be changed
    /// SetAccessLevel -> to level the user access should be set
    /// </param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Change access level to user",typeof(UserChangeAccessResponse))]
    [Route("access/{id}")]
    public async Task<IActionResult> ChangeAccessLevelOfUser([FromBody] ChangeUserAccessRequest request)
    {
        var response = await _userService.SetAccessLevel(request);
        return Ok(response);
    }

    /// <summary>
    /// Remove user from organization by userId (Admin endpoint)
    /// </summary>
    /// <param name="userId"> id of a user that will be removed </param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    //[AllowAnonymous] // For testing
    [HttpDelete]
    [SwaggerResponse((int) HttpStatusCode.OK, "Remove user from organization")]
    [Route("Remove/user/{userId}")]
    public async Task<IActionResult> RemoveUserFromOrganization(string userId)
    {
        await _userService.RemoveUserById(userId);
        return Ok($"user by id: {userId} removed");
    }

}