namespace Domain.Exceptions;

public class UserForbiddenAction : Exception
{
    public UserForbiddenAction(string message) : base(message)
    {
        
    }
}