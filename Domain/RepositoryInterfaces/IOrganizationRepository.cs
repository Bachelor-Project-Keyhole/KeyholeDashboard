﻿namespace Domain.RepositoryInterfaces;

public interface IOrganizationRepository
{
    Task<bool> OrganizationExists(string organizationId);
    Task Insert(Organization.Organization organizationToInsert);
}