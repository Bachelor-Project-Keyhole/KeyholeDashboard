namespace Domain.Exceptions;

public class UserNotFoundException :  HttpRequestException
{
    public UserNotFoundException(string message): base(message)
    {}
}