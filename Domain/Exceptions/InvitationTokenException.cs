namespace Domain.Exceptions;

public class InvitationTokenException : Exception
{
    public InvitationTokenException(string message): base(message)
    {}
}