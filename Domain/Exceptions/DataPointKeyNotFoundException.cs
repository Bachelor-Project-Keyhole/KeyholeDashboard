namespace Domain.Exceptions;

public class DataPointKeyNotFoundException : Exception
{
    public DataPointKeyNotFoundException(string message) : base(message)
    {}
}