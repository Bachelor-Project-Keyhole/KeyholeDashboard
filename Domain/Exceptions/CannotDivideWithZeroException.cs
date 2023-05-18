namespace Domain.Exceptions;

public class CannotDivideWithZeroException : Exception
{
    private new const string Message = "Cannot divide with zero";
    
    public CannotDivideWithZeroException() : base(Message)
    {}
}