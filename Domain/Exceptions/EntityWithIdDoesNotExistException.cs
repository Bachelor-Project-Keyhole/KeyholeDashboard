namespace Domain.Exceptions;

public class EntityWithIdDoesNotExistException : Exception
{
    public EntityWithIdDoesNotExistException(string message) : base(message) 
    {}
}