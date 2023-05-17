namespace Domain.Exceptions;

public class CannotDivideWithZeroException : Exception
{
    private const string Message = "Cannot divide with zero";
    
    public CannotDivideWithZeroException() : base(Message)
    {}
}