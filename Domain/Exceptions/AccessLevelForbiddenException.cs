namespace Domain.Exceptions;

public class AccessLevelForbiddenException : Exception
{
    public AccessLevelForbiddenException(string message): base(message)
    {}
}