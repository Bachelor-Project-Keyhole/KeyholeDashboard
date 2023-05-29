using System.Net;
using System.Transactions;
using Application.Email.EmailService;
using Application.JWT.Authorization;
using Application.Organization;
using Application.User.Model;
using Application.User.UserService;
using AutoMapper;
using Contracts.v1.Organization;
using Domain.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Controllers.Shared;

namespace WebApi.Controllers.V1.Organization;


[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
public class OrganizationController : BaseApiController
{
    private readonly IOrganizationApplicationService _organizationApplicationService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public OrganizationController(
        IOrganizationApplicationService organizationApplicationService,
        IUserService userService,
        IEmailService emailService,
        IMapper mapper)
    {
        _organizationApplicationService = organizationApplicationService;
        _userService = userService;
        _emailService = emailService;
        _mapper = mapper;
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
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Invite user into the organization")]
    [Route("invite/email")]
    public async Task<IActionResult> InviteUserToOrganization([FromBody]OrganizationUserInviteRequest request)
    {
        var link = await _organizationApplicationService.InviteUser(request);
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
                var invitation = await _organizationApplicationService.TokenValidity(token);
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
    /// Fetch detailed info on organization by Id (Any auth level required)
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get organization by Id", typeof(OrganizationDetailedResponse))]
    [Route("{organizationId}")]
    public async Task<IActionResult> GetOrganizationById(string organizationId)
    {
        var response = await _organizationApplicationService.GetOrganizationById(organizationId);
        return Ok(response);
    }
    
    /// <summary>
    /// Get all users of the organization (Any auth level required)
    /// </summary>
    /// <param name="organizationId"> id of an organization, the users are assigned to</param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin, UserAccessLevel.Editor, UserAccessLevel.Viewer)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get all users of the organization", typeof(AllUsersOfOrganizationResponse))]
    [Route("users/{organizationId}")]
    public async Task<IActionResult> GetAllUsersOfOrganization(string organizationId)
    {
        var response = await _userService.GetAllUsers(organizationId);
        return Ok(response);
    }
    
    /// <summary>
    /// Get user invitation by Id (Admin)
    /// </summary>
    /// <param name="invitationId"> Id of an invitation doc</param>
    /// <param name="organizationId"> Just to make sure all the invitations that can be fetched belongs in organization scope</param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get user invitation by Id", typeof(PendingUserInvitationResponse))]
    [Route("invitationId/{invitationId}/organizationId/{organizationId}")]
    public async Task<IActionResult> GetInvitationById(string invitationId, string organizationId)
    {
        var response = await _organizationApplicationService.GetInvitationById(invitationId, organizationId);
        return Ok(_mapper.Map<PendingUserInvitationResponse>(response));
    }

    /// <summary>
    /// Get user invitation by email (Admin)
    /// </summary>
    /// <param name="email">email to which invitation was sent to</param>
    /// <param name="organizationId">Just to make sure all the invitations that can be fetched belongs in organization scope</param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get user invitation by email", typeof(PendingUserInvitationResponse))]
    [Route("invitation/email/{email}/organizationId/{organizationId}")]
    public async Task<IActionResult> GetInvitationByEmail(string email, string organizationId)
    {
        var response = await _organizationApplicationService.GetInvitationByEmail(email, organizationId);
        return Ok(_mapper.Map<PendingUserInvitationResponse>(response));
    }
    
    /// <summary>
    /// Get user invitation by organization Id (Admin)
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    [HttpGet]
    [SwaggerResponse((int) HttpStatusCode.OK, "Get user invitation by organization Id", typeof(List<PendingUserInvitationResponse>))]
    [Route("invitation/organization/{organizationId}")]
    public async Task<IActionResult> GetInvitationByOrganizationId(string organizationId)
    {
        var response = await _organizationApplicationService.GetInvitationByOrganizationId(organizationId);
        return Ok(_mapper.Map<List<PendingUserInvitationResponse>>(response));
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
        var response = await _organizationApplicationService.UpdateOrganization(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Change access level to user
    /// </summary>
    /// <param name="request">
    /// AdminId -> admin that wants to change access to user
    /// SetAccessLevel -> to level the user access should be set
    /// </param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Change access level to user",typeof(UserChangeAccessResponse))]
    [Route("access")]
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
    [HttpDelete]
    [SwaggerResponse((int) HttpStatusCode.OK, "Remove user from organization")]
    [Route("Remove/user/{userId}")]
    public async Task<IActionResult> RemoveUserFromOrganization(string userId)
    {
        await _userService.RemoveUserById(userId);
        return Ok($"user by id: {userId} removed");
    }

    /// <summary>
    /// Remove invitation by Id (Admin)
    /// </summary>
    /// <param name="invitationId">invitation id</param>
    /// <param name="organizationId">Just to make sure all the invitations that can be fetched belongs in organization scope</param>
    /// <returns></returns>
    [Authorization(UserAccessLevel.Admin)]
    [HttpDelete]
    [SwaggerResponse((int) HttpStatusCode.OK, "Remove invitation by Id")]
    [Route("invitation/id/{invitationId}/organizationId/{organizationId}")]
    public async Task<IActionResult> RemoveInvitationById(string invitationId, string organizationId)
    {
        await _organizationApplicationService.RemoveInvitationById(invitationId, organizationId);
        return Ok();
    }

    /// <summary>
    /// Forgot password function send email
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Forgot password function")]
    [Route("password/send/email")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var response = await _userService.ForgotPassword(request);
        return Ok(response);
    }

    /// <summary>
    /// Reset password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [SwaggerResponse((int) HttpStatusCode.OK, "Reset password")]
    [Route("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReset request)
    {
        await _userService.ResetPassword(request.Token, request.Password);
        return Ok("Password changed");
    }
    
    

}