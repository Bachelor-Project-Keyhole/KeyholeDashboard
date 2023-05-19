namespace Domain.Exceptions;

public class OrganizationNotFoundException : Exception
{
    public OrganizationNotFoundException(string organizationId): base($"Organization with Id: {organizationId} was not found")
    {}
}