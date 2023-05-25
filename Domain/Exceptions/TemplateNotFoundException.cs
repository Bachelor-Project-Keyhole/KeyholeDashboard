namespace Domain.Exceptions;

public class TemplateNotFoundException : Exception
{
    public TemplateNotFoundException(string message) : base(message)
    {}
}