﻿namespace Contracts.v1.Organization;

public class UserRegistrationResponse
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string OrganizationId { get; set; }
    public string RegistrationDate { get; set; }
}