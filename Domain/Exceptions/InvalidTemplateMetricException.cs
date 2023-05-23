namespace Domain.Exceptions;

public class InvalidTemplateMetricException : Exception
{
    public InvalidTemplateMetricException(string message) : base(message)
    {}
}