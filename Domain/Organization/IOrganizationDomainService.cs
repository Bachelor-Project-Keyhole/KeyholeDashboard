namespace Domain.Organization;

public interface IOrganizationDomainService
{
    Task<bool> OrganizationExists(string organizationId);
    Task<Organization> GetOrganizationById(string organizationId);
    Task<Organization> GetOrganizationByApiKey(string apiKey);
    Task Insert(Organization organization);
    Task Update(Organization organization);
    
    Task<List<OrganizationUserInvite.OrganizationUserInvites>> GetAllInvitesByOrganizationId(string organizationId);
    Task<OrganizationUserInvite.OrganizationUserInvites> GetInvitationById(string invitationId, string organizationId);
    Task<OrganizationUserInvite.OrganizationUserInvites> GetInvitationByEmail(string email, string organizationId);
    Task<OrganizationUserInvite.OrganizationUserInvites> GetInvitationByToken(string token);
    Task InsertInviteUser(OrganizationUserInvite.OrganizationUserInvites invite);
    Task UpdateUserInvite(OrganizationUserInvite.OrganizationUserInvites invite);
    Task RemoveInvitationByToken(string token);
    Task RemoveInvitationById(string invitationId, string organizationId);
}