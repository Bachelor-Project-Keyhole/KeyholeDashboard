﻿using Contracts.v1.Organization;
using Domain.Organization.OrganizationUserInvite;

namespace Application.Organization;

public interface IOrganizationService
{
    Task<OrganizationDetailedResponse> GetOrganizationById(string organizationId);
    Task<(string, string)> InviteUser(OrganizationUserInviteRequest request);
    Task<OrganizationUserInvites> TokenValidity(string token);
    Task<OrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request);
}