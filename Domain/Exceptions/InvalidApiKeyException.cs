namespace Domain.Exceptions;

public class InvalidApiKeyException : Exception
{
    public InvalidApiKeyException() : base("Invalid API key")
    {}
}