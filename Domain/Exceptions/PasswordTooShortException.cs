namespace Domain.Exceptions;

public class PasswordTooShortException : Exception
{
    public PasswordTooShortException(string message): base(message)
    {}
}