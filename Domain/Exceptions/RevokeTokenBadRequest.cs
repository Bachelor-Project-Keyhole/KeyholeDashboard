namespace Domain.Exceptions;

public class RevokeTokenBadRequest : Exception
{
    public RevokeTokenBadRequest(string message): base(message)
    {}
}