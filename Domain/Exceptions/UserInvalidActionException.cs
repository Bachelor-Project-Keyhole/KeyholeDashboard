namespace Domain.Exceptions;

public class UserInvalidActionException : Exception
{
    public UserInvalidActionException(string message) : base(message)
    {}
}