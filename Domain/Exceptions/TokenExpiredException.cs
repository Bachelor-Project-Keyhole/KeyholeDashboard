namespace Domain.Exceptions;

public class TokenExpiredException : Exception
{
    public TokenExpiredException(string message) : base(message)
    {}
}