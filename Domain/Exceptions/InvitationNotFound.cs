namespace Domain.Exceptions;

public class InvitationNotFound : Exception
{
    public InvitationNotFound(string message): base(message)
    {}
}